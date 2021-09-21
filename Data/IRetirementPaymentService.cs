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
    public interface IRetirementPaymentService
    {
        Task<RetirementPayment> AddRetirementPayment(int UserId, int RoleId, RetirementPaymentViewModel retirementPaymentViewModel, List<RetirementCashPaymentsViewModel> retirementCashPaymentsViewModels, int cashAdvanceId);
        Task<RetirementPayment> CreateRetirementPayment(int UserId, int RoleId, RetirementPaymentViewModel retirementPaymentViewModel, List<RetirementCashPaymentsViewModel> retirementCashPaymentsViewModels, int cashAdvanceId);

        Task<IEnumerable<RetirementPayment>> GetApprovedRetirementPayment(int UserId);
        Task<IEnumerable<RetirementPayment>> GetUnApprovedRetirementPayment(int RoleId);
        Task<IEnumerable<RetirementPayment>> GetRetirePaymentsForRole(int RoleId, int userId);
        Task<RetirementPayment> PerformActionOnRetirePayment(int retirementPaymentId, int UserId, int RoleId, string Comment, ActionPerformed actionPerformed);
        Task<RetirementPayment> GetRetirePayment(int retirementPaymentId, int UserId);
        Task<List<RetirementCashBookPayments>> GetCashRetirePayment(int retirementPaymentId);
        Task<RetirementPayment> EditRetirePayment(RetirementPayment retirementPayment, List<RetirementCashBookPayments> retirementCashBookPayments, int UserId, int RoleId, string comment);
        Task<List<RetirementPaymentAction>> GetRetirePaymentAction(int retirementPaymentId);
        Task<List<RetirementPaymentFile>> GetRetirePaymentFiles(int retirementPaymentId);
        Task<RetirementPaymentFile> DeleteRetirePaymentFile(int retirementPaymentFileId, int UserId, int RoleId);
        Task<RetirementCashBookPayments> DeleteRetirePaymentCashBook(int UserId, int RoleId, int retirementPaymentId);
        //Task<Voucher> EditVoucherFiles(int voucherId, VoucherFileViewModel uploadFiles, int UserId, int RoleId);
        Task<RetirementPayment> AddRetirementFiles(int retirementPaymentId, List<IFormFile> files, int UserId, int RoleId);
        Task<IEnumerable<RetirementPayment>> GetAllRetirementPayments(int UserId);
        Task<IEnumerable<CashAdvance>> GetUnRetiredCashAdvanceForUser(int UserId);
        //Task<DashBoardViewModel> DashBoard();
        Task<IEnumerable<RetirementPayment>> GetAllRetirementPaymentsForUser(int UserId);
    }
}
