/**
 * Account Mapper
 * Maps API response to Account model and applies enums
 */

const AccountMapper = (function() {
    // Map single account from API
    function fromApi(apiData) {
        if (!apiData) return null;

        const account = new Account(apiData);

        // Apply enum mappings
        account.roleText = RoleHelper.getText(apiData.role);
        account.roleIcon = RoleHelper.getIcon(apiData.role);
        account.statusText = AccountStatusHelper.getText(apiData.status);
        account.statusBadge = AccountStatusHelper.getBadge(apiData.status);

        // Format dates
        account.formattedCreatedAt = formatDate(apiData.createdAt);
        account.formattedLastLogin = formatDateTime(apiData.lastLoginAt);

        return account;
    }

    // Map account list from API
    function fromApiList(apiDataList) {
        if (!apiDataList || !Array.isArray(apiDataList)) return [];
        return apiDataList.map(fromApi);
    }

    // Map paged response
    function fromPagedResponse(pagedResponse) {
        if (!pagedResponse) return null;

        return {
            ...pagedResponse,
            data: fromApiList(pagedResponse.data),
            items: fromApiList(pagedResponse.items || pagedResponse.data)
        };
    }

    // Map login response to UserInfo
    function fromLoginResponse(loginResponse) {
        if (!loginResponse || !loginResponse.data) return null;

        return new UserInfo({
            userId: loginResponse.data.userId || 0,
            username: loginResponse.data.username,
            email: loginResponse.data.email,
            fullName: loginResponse.data.fullName,
            role: loginResponse.data.role,
            avatarUrl: loginResponse.data.avatarUrl
        });
    }

    // Map to API create DTO
    function toCreateDto(account) {
        return {
            username: account.username?.trim(),
            email: account.email?.trim(),
            password: account.password || '',
            role: account.role,
            fullName: account.fullName?.trim(),
            phoneNumber: account.phoneNumber?.trim(),
            status: account.status || AccountStatus.HOAT_DONG
        };
    }

    // Map to API update DTO
    function toUpdateDto(account) {
        return {
            id: account.id,
            fullName: account.fullName?.trim(),
            phoneNumber: account.phoneNumber?.trim(),
            avatarUrl: account.avatarUrl,
            status: account.status
        };
    }

    // Format date helper
    function formatDate(dateString) {
        if (!dateString) return '';
        return new Date(dateString).toLocaleDateString('vi-VN');
    }

    // Format date time helper
    function formatDateTime(dateString) {
        if (!dateString) return '';
        return new Date(dateString).toLocaleString('vi-VN');
    }

    // Public API
    return {
        fromApi,
        fromApiList,
        fromPagedResponse,
        fromLoginResponse,
        toCreateDto,
        toUpdateDto,
        formatDate,
        formatDateTime
    };
})();

window.AccountMapper = AccountMapper;
