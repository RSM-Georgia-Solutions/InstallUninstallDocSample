using ApplicationCore.Enums;
using ApplicationCore.Models.TransferDocument;
using ApplicationCore.ServiceInterfaces;
using FixedAssets.Domain.Services;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallDocSample
{
    class TransferSample
    {
        public ITransferDocumentService transferService { get; set; }

        public TransferSample(SAPbobsCOM.Company oCompany)
        {
            transferService = new TransferDocumentService(oCompany);
        }

        /// <summary>
        /// გადაადგილების დოკუემნტის შენახვა
        /// </summary>
        public void CreateTransferDocTest()
        {
            var transferDoc = new TransferDoc();
            transferDoc.EmpID = "1";
            transferDoc.TransferType = TFTransferTypes.FromWarehouse;
            transferDoc.TransferStatus = TFTransferStatusTypes.Started;
            transferDoc.FromWhsCode = "აწყ.ახალ";
            transferDoc.ToWhsCode = "აწყ.ახალ";
            transferDoc.DocDate = DateTime.Now.Date;
            transferDoc.TerminalCode = "8278382";
            
            transferDoc.Lines.Add(new TransferDocLines
            {
                ItemCode = "000011",
                ItemName = "ტერმინალი",
                TerminalCode = "8278382",
                FromLocCode = "jk",
                ToLocCode = "ჯკჯ",
                VirtualWhsCode = "აწყ.ახალ",
                Quantity = 1,
                UoMGroupEntry = 1,
                IUoM = "ცალი",
                IUoMEntry = 1,
            });

            var res = transferService.PostDocument(transferDoc);
            //Assert.IsTrue(res.Code == 0);
        }

        public void EndTransferDocTest()
        {
            var res = transferService.EndTransfer(135);
            //Assert.IsTrue(res.Code == 0);
        }


        /// <summary>
        /// გადაადგილების დოკუმენტიდან ზედნადების უდოს შენახვა
        /// </summary>
        public void SaveWaybillTest()
        {

            var res = transferService.GetWaybillFromTransfer(135);
            //Assert.IsTrue(res.Code == 0);
        }



        /// <summary>
        /// ზედნადების უდოს განახლება
        /// </summary>
        public void UpdateWaybillTest()
        {

            var wb = transferService.GetWaybill(135);
            wb.DriverTIN = "60001137503";
            wb.CarNumber = "ww505dd";
            var res = transferService.UpdateWaybillUDO(wb);
            var wb2 = transferService.GetWaybill(135);

            //Assert.IsTrue(res.Code == 0);
        }


        /// <summary>
        /// RS.ge ზე ზედნადების ატვირთვა
        /// </summary>
        public void PostWaybillTest()
        {
            var uploadres = transferService.UploadWaybillOnRS(135);

            //Assert.IsTrue(res.Code == 0);
            //Assert.IsTrue(uploadres.Code == 0);
        }


        /// <summary>
        /// RS.ge სთან სინქრონიზაცია
        /// </summary>
        public void SyncWaybillTest()
        {
            var res = transferService.SyncWaybill(135);
            //Assert.IsTrue(res.Code == 0);
        }

    }
}
