using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 报价确认/精确报价共用的 CRM 批量选择状态，应用时实时获取当前勾选行。
    /// 输入文字仅改变搜索条件；选择候选项才调用各行的机构/联系人联动逻辑，不修改未勾选行。
    /// </summary>
    public partial class CrmBatchEditor : ObservableObject
    {
        private readonly Func<IEnumerable<QuoteConfirmRowViewModel>> selectedRows;
        private bool selecting;

        public CrmBatchEditor(Func<IEnumerable<QuoteConfirmRowViewModel>> selectedRows)
        {
            this.selectedRows = selectedRows;
        }

        [ObservableProperty] private string _institutionText = string.Empty;
        [ObservableProperty] private string _contactText = string.Empty;
        [ObservableProperty] private string _institutionId;

        partial void OnInstitutionTextChanged(string value)
        {
            if (selecting) return;
            InstitutionId = null;
            ContactText = string.Empty;
        }

        [RelayCommand]
        private void SelectInstitution(CrmChoice choice)
        {
            Apply(choice, false);
        }

        [RelayCommand]
        private void SelectContact(CrmChoice choice)
        {
            Apply(choice, true);
        }

        private void Apply(CrmChoice choice, bool contact)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.InstitutionId)
                || (contact && string.IsNullOrWhiteSpace(choice.ContactId))) return;
            var rows = selectedRows().ToList();
            if (rows.Count == 0)
            {
                ToastService.Show("请先勾选要批量修改的报价。", ToastKind.Warning);
                return;
            }
            selecting = true;
            try
            {
                if (InstitutionId != choice.InstitutionId) ContactText = string.Empty;
                InstitutionId = choice.InstitutionId;
                InstitutionText = choice.InstitutionName;
                if (contact) ContactText = choice.ContactName;
                foreach (var row in rows)
                {
                    if (contact) row.SelectContact(choice);
                    else row.SelectInstitution(choice);
                }
            }
            finally
            {
                selecting = false;
            }
        }
    }
}
