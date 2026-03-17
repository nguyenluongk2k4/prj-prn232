/**
 * API Response Model
 * Standard response format from API
 */

class ApiResponse {
    constructor(data) {
        this.success = data?.success ?? false;
        this.message = data?.message ?? '';
        this.data = data?.data ?? null;
        this.timestamp = new Date().toISOString();
    }

    get isSuccess() {
        return this.success === true;
    }

    get isError() {
        return this.success === false;
    }
}

/**
 * Paged Response Model
 * For paginated API responses
 */
class PagedResponse extends ApiResponse {
    constructor(data) {
        super(data);
        this.pageNumber = data?.pageNumber ?? 1;
        this.pageSize = data?.pageSize ?? 10;
        this.totalRecords = data?.totalRecords ?? 0;
        this.totalPages = data?.totalPages ?? 0;
        this.hasPrevious = data?.hasPrevious ?? false;
        this.hasNext = data?.hasNext ?? false;
    }

    get isEmpty() {
        return !this.data || this.data.length === 0;
    }

    get hasData() {
        return !this.isEmpty;
    }
}

/**
 * Paged Request Model
 * For sending pagination parameters to API
 */
class PagedRequest {
    constructor(pageNumber = 1, pageSize = 10, filters = {}) {
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
        this.filters = filters;
    }

    toParams() {
        return {
            pageNumber: this.pageNumber,
            pageSize: this.pageSize,
            ...this.filters
        };
    }

    static create(page = 1, size = 10, filters = {}) {
        return new PagedRequest(page, size, filters);
    }
}

/**
 * Error Response Model
 */
class ErrorResponse {
    constructor(error) {
        this.success = false;
        this.message = error?.message ?? 'An error occurred';
        this.statusCode = error?.statusCode ?? 500;
        this.errors = error?.errors ?? null;
        this.timestamp = new Date().toISOString();
    }

    get hasErrors() {
        return this.errors && Object.keys(this.errors).length > 0;
    }
}

window.ApiResponse = ApiResponse;
window.PagedResponse = PagedResponse;
window.PagedRequest = PagedRequest;
window.ErrorResponse = ErrorResponse;
