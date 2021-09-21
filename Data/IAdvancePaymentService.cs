using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.Models.Enums;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Data
{
    public interface IAdvancePaymentService
    {
        Task<CashAdvance> AddNewCashAdvance(int UserId, int RoleId, CashAdvanceViewModel cashAdvanceViewModel, List<CashAdvancePaymentsViewModel> cashAdvancePaymentsViewModels);

        Task<IEnumerable<CashAdvance>> GetActiveCashAdvance(int UserId);
        Task<IEnumerable<CashAdvance>> GetInActiveCashAdvance(int UserId);
        Task<IEnumerable<CashAdvance>> GetCashAdvanceForRole(int RoleId, int userId);
        Task<CashAdvance> PerformActionOnCashAdvance(int cashAdvanceId, int UserId, int RoleId,  string Comment, ActionPerformed actionPerformed);
        Task<CashAdvance> GetCashAdvance(int pettyCashId, int UserId);
        Task<List<CashAdvancePayment>> GetCashAdvancePayments(int cashAdvanceId);
        Task<CashAdvance> EditCashAdvance(CashAdvance cashAdvance, List<CashAdvancePayment> cashAdvancePayments, int UserId, int RoleId, string comment);
        Task<List<Models.CashAdvanceAction>> GetCashAdvanceAction(int cashAdvanceId);
        Task<List<CashAdvanceFile>> GetCashAdvanceFiles(int cashAdvanceId);
        Task<CashAdvanceFile> DeleteCashAdvanceFile(int CashAdvanceFileId, int UserId, int RoleId);
        Task<CashAdvancePayment> DeleteAdvancePayments( int UserId, int RoleId, int CashAdvanceId);
        //Task<Voucher> EditVoucherFiles(int voucherId, VoucherFileViewModel uploadFiles, int UserId, int RoleId);
        Task<CashAdvance> AddCashAdvanceFiles(int cashAdvanceId, List<IFormFile> files, int UserId, int RoleId);
        Task<IEnumerable<CashAdvance>> GetAllCashAdvance(int UserId);
        Task<IEnumerable<CashAdvance>> GetAllCashAdvanceCreatedByUser(int UserId);
        //Task<DashBoardViewModel> DashBoard();
    }
}
