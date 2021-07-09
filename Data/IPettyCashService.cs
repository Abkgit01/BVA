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
    public interface IPettyCashService
    {

        Task<PettyCash> AddNewPettyCash(int UserId, int RoleId, PettyCashViewModel pettyCashViewModel);

        Task<IEnumerable<PettyCash>> GetApprovedPettyCash(int UserId);
        Task<IEnumerable<PettyCash>> GetUnApprovedPettyCash(int UserId);
        Task<IEnumerable<PettyCash>> GetPendingPettyCashForUser(int RoleId, int userId);
        Task<PettyCash> PerformActionOnPettyCash(int PettyCashId, int UserId, int RoleId, string Comment, ActionPerformed actionPerformed);
        Task<PettyCash> GetPettyCash(int pettyCashId, int UserId);
        //Task<List<PettyCashBook>> GetPettyCashBook(int pettyCashId);
        Task<PettyCash> EditPettyCash(PettyCash pettyCash, int UserId, int RoleId, string comment);
        Task<List<PettyCashAction>> GetPettyCashActions(int PettyCashId);
        Task<List<PettyCashFile>> GetPettyCashFiles(int PettyCashId);
        Task<PettyCashFile> DeletePettyCashFile(int PettyCashFileId, int UserId, int RoleId);
        //Task<PettyCashBook> DeletePettyCashBook(int UserId, int RoleId, int PettyCashId);
        Task<PettyCash> AddPettyCashFiles(int PettyCashId, List<IFormFile> files, int UserId, int RoleId);
        Task<IEnumerable<PettyCash>> GetAllPettyCash(int UserId);
        Task<IEnumerable<PettyCash>> GetAllPettyCashForUser(int UserId);
    }
}
