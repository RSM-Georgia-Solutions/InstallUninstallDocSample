using ApplicationCore.Enums;
using ApplicationCore.Meta;
using ApplicationCore.Models;
using ApplicationCore.Models.InstallDocument;
using ApplicationCore.Models.UninstallDocument;
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
        public bool InstallDocPost(InstallDoc installDoc, SAPbobsCOM.Company oCompany)
        {
            var installService = new InstallDocumentService(oCompany);

            var doc = new InstallDoc();
            doc.TerminalCode = "292929";
            doc.WhsCode = "აწყ.ახალ";
            doc.ItemCode = "TerminalTest3";
            doc.DocDate = DateTime.Now;
            doc.TerminalLocation = "5";
            doc.EmpID = 1;
            doc.Remark = "remarks";

            var line = new InstallDocLines();
            line.WhsLocation = 5;
            line.GroupCode = "104";
            line.GroupName = "ფულის მიმღები";
            line.ItemCode = "001-01";
            line.ItemName = "ფულის მიმღები MEI ADVANCE";
            line.SerialNumber = "FA000015";
            line.WhsCode = "აწყ.დაზი";
            line.WhsName = "აწყობის საწყობი - დაზიანებული";
            line.Quantity = 1;
            line.SysSerial = 20;

            doc.Lines.Add(line);

            var res = installService.PostDocument(doc);
            
            //ASSERT 
            return res.Code == 0;
        }

        public bool UnInstallDocPost(InstallDoc installDoc, SAPbobsCOM.Company oCompany)
        {
            var uninstallService = new UninstallDocumentService(oCompany);

            var doc = new UninstallDoc();
            doc.TerminalCode = "292929";
            doc.WhsCode = "აწყ.ახალ";
            doc.ItemCode = "TerminalTest3";
            doc.DocDate = DateTime.Now;
            doc.TerminalLocation = "5";
            doc.EmpID = 1;
            doc.Remark = "remarks";

            var line = new UninstallDocLines();
            line.WhsLocation = 5;
            line.GroupCode = "104";
            line.GroupName = "ფულის მიმღები";
            line.ItemCode = "001-01";
            line.ItemName = "ფულის მიმღები MEI ADVANCE";
            line.SerialNumber = "FA000015";
            line.WhsCode = "აწყ.დაზი";
            line.WhsName = "აწყობის საწყობი - დაზიანებული";
            line.Quantity = 1;
            line.SysSerial = 20;

            doc.Lines.Add(line);

            var res = uninstallService.PostDocument(doc);

            //ASSERT 
            return res.Code == 0;
        }

    }
}
