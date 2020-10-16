using ApplicationCore.Enums;
using ApplicationCore.Meta;
using ApplicationCore.Models;
using ApplicationCore.Models.InstallDocument;
using ApplicationCore.Provider;
using ApplicationCore.ServiceInterfaces;
using DataAccess.DI.Providers;
using FixedAssets.Domain.Services;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallDocSample
{
    class InUnDocument
    {
        public bool PostDocument(InstallDoc installDoc, SAPbobsCOM.Company oCompany)
        {
            //ვალიდაცია
            var res = Validate(installDoc, oCompany);
            if (res.Code != 0)
                return false;

            var settingsProvider = new SettingsModelProvider(oCompany);
            SettingsModel Settings = settingsProvider.GetSettings();

            IGoodsIssueProvider goodsIssueProvider = new GoodsIssueProvider(oCompany);
            IJournalEntriesProvider journalEntryProvider = new JournalEntriesProvider(oCompany);
            IInstallDocProvider installDocProvider = new InstallDocProvider(oCompany, UDONames.InstallUDO);
            IUninstallDocumentService uninstallDocService = new UninstallDocumentService(oCompany);

            //გატარებები


            var InstallDocService = new InstallDocumentService(oCompany);
            var LastResult = new Result();

            //ტრანზაქცია 
            if (!oCompany.InTransaction)
                oCompany.StartTransaction();

            try
            {
                //აქ უნდა მოხდეს საპში installDoc დამატება ამის შემდეგ იწყება გატარებების დაპოსტვა
                //installDoc არის საპის UDO
                installDocProvider.Save(installDoc);


                var giResult = new Result();
                var capResult = new Result();
                var grResult = new Result();

                if (installDoc.DocType == TFDocDypesEnum.Install)
                {
                    //goods issue დაპოსტვა
                    giResult = InstallDocService.PostGoodsIssueDoc(installDoc, Settings);

                    if (giResult.Code != 0)
                    {
                        if (oCompany.InTransaction)
                            oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                        LastResult = giResult;
                        return false;
                    }

                    var invGenExitDoc = goodsIssueProvider.Get(giResult.DocEntry);
                    var vj1 = journalEntryProvider.GetJournalEntry(invGenExitDoc.TransNum);

                    //თუ კაპიტალიზაცია იპოსტება + ლოგიკა
                    //installDoc.Lines.First() - მხოლოდ ერთი ხაზი შეიძლება ქონდეს
                    if (installDocProvider.PostCapitalizationCheck(installDoc.TerminalCode, installDoc.Lines.First().GroupCode))
                    {
                        capResult = InstallDocService.PostCapitalizationDoc(installDoc, Settings, vj1);

                        if (capResult.Code != 0)
                        {
                            if (oCompany.InTransaction)
                                oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                            LastResult = capResult;
                            return false;
                        }
                    }
                    else
                    {
                        //Helpers.ShowWarning($"Capitalization დოკუმენტი არ დაიპოსტა, რადგან არსებული აითემ ჯგუფის აითემი დგას უკვე ტერმინალზე");
                    }


                }
                else //unistall doc
                {
                    grResult = uninstallDocService.PostGoodsReceipt(installDoc, Settings);
                    uninstallDocService.UpdateAssetItem(installDoc);
                    if (grResult.Code != 0)
                    {
                        if (oCompany.InTransaction)
                            oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                        LastResult = grResult;
                        return false;
                    }

                }

                if (oCompany.InTransaction)
                    oCompany.EndTransaction(BoWfTransOpt.wf_Commit);

                return true;
            }
            catch (Exception ex)
            {
                if (oCompany.InTransaction)
                    oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);

                LastResult = new Result() { ErrorDescription = ex.Message };
                return false;
            }
        }


        public Result Validate(InstallDoc installDoc, SAPbobsCOM.Company oCompany)
        {
            var lastResult = new Result();
            if (installDoc.DocDate.Year == 1899)
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "თარიღის მითითება აუცილებელია";
                return lastResult;
            }
            else if (installDoc.EmpID == 0)
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "აირჩიეთ თანამშრომელი";
                return lastResult;
            }
            else if (installDoc.WhsCode == "")
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "აირჩიეთ საწყობი";
                return lastResult;
            }
            else if (installDoc.TerminalCode == "")
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "აირჩიეთ ტერმინალი";
                return lastResult;
            }
            else if (installDoc.Lines.First().GroupCode == "")
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "აირჩიეთ ჯგუფი";
                return lastResult;
            }
            else if (installDoc.Lines.First().WhsCode == "")
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "აირჩიეთ საწყობი";
                return lastResult;
            }
            else if (installDoc.Lines.First().IntrSerial == "")
            {
                lastResult.Code = -1;
                lastResult.ErrorDescription = "აირჩიეთ სერიული ნომერი";
                return lastResult;
            }



            if (installDoc.DocType == TFDocDypesEnum.Install)
            {
                var groupCode = installDoc.Lines.First().GroupCode;
                IInstallDocProvider installDocProvider = new InstallDocProvider(oCompany, UDONames.InstallUDO);

                if (!installDocProvider.HaveAvailableItemQTY(installDoc.TerminalCode, groupCode))
                {
                    if (oCompany.InTransaction)
                        oCompany.EndTransaction(BoWfTransOpt.wf_RollBack);
                    lastResult.Code = -6;
                    lastResult.ErrorDescription = $"'{groupCode}' ჯგუფის რაოდენობა აღემატება BOM-ში გაწერილ მაქსიმალურ რაოდენობას.";
                }
            }


            return lastResult;
        }

    }
}
