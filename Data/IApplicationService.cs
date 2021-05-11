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
    public interface IApplicationService
    {
        Task<Voucher> AddNewVoucher(int UserId, int RoleId, VoucherViewModel voucherViewModel, List<CashBookViewModel> cashBookViewModels);

        Task<IEnumerable<Voucher>> GetActiveVouchers(string RoleName);
        Task<IEnumerable<Voucher>> GetInActiveVouchers(int RoleId);
        Task<IEnumerable<Voucher>> GetVouchersForRole(int RoleId, int userId);
        Task<Voucher> PerformActionOnVoucher(int VoucherId, int UserId, int RoleId,
            string Comment, ActionPerformed actionPerformed);
        Task<Voucher> GetVoucher(int Id, string RoleName);
        Task<List<CashBook>> GetCashbook(int Id);
        Task<Voucher> EditVoucher(Voucher voucher, List<CashBook> cashBooks, int UserId, int RoleId, string comment);
        Task<List<Models.Action>> GetVoucherActions(int roleId);
        Task<List<VoucherFile>> GetVoucherFiles(int Id);
        Task<VoucherFile> DeleteVoucherFiles(int Id, int UserId, int RoleId);
        Task<CashBook> DeleteCashBook(int Id, int UserId, int RoleId);
        Task<Voucher> EditVoucherFiles(int voucherId, VoucherFileViewModel uploadFiles , int UserId, int RoleId);
        Task<Voucher> AddVoucherFiles(int voucherId, List<IFormFile> files, int UserId, int RoleId);
        Task<IEnumerable<Voucher>> GetAllVouchers(string RoleName);
        Task<DashBoardViewModel> DashBoard();

    }
}
