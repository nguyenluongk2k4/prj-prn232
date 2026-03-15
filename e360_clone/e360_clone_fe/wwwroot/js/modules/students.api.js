// Student API Service
const StudentApi = {
    async getAll(page = 1, pageSize = 10, searchTerm = '') {
        const params = { pageNumber: page, pageSize };
        if (searchTerm) params.searchTerm = searchTerm;
        return await api.get('/students', params);
    },

    async getById(id) {
        return await api.get(`/students/${id}`);
    },

    async create(student) {
        return await api.post('/students', student);
    },

    async update(id, student) {
        return await api.put(`/students/${id}`, student);
    },

    async delete(id) {
        return await api.delete(`/students/${id}`);
    }
};
