path = r"e360_clone_api\\Controllers\\StudentsController.cs"
text = open(path, encoding='utf-8', errors='replace').read()
old_block = '''            return Ok(new PagedResponse<Student>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch sinh viÃªn thÃ nh cÃ´ng",
                Data = data.ToList(),
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });'''
new_block = '''            var students = data.ToList();
            var accounts = await _accountRepository.GetByStudentIdsAsync(students.Select(s => s.Id));
            var avatarMap = accounts
                .Where(a => a.StudentId.HasValue)
                .GroupBy(a => a.StudentId!.Value)
                .ToDictionary(g => g.Key, g => g.First().AvatarUrl);

            var dtoList = students.Select(s => new StudentDto
            {
                Id = s.Id,
                StudentCode = s.StudentCode,
                FullName = s.FullName,
                DateOfBirth = s.DateOfBirth,
                Gender = s.Gender,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Address = s.Address,
                ClassId = s.ClassId,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                AvatarUrl = avatarMap.TryGetValue(s.Id, out var avatar) ? avatar : null
            }).ToList();

            return Ok(new PagedResponse<StudentDto>
            {
                Success = true,
                Message = "Láº¥y danh sÃ¡ch sinh viÃªn thÃ nh cÃ´ng",
                Data = dtoList,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalRecords = totalRecords
            });'''
text = text.replace(old_block, new_block)
old_return = '            return HandleResult(student, "Láº¥y thÃ´ng tin sinh viÃªn thÃ nh cÃ´ng");'
new_return = '''            var account = (await _accountRepository.GetByStudentIdsAsync(new[] { id })).FirstOrDefault();
            var dto = new StudentDto
            {
                Id = student.Id,
                StudentCode = student.StudentCode,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                Address = student.Address,
                ClassId = student.ClassId,
                Status = student.Status,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt,
                AvatarUrl = account?.AvatarUrl
            };

            return HandleResult(dto, "Láº¥y thÃ´ng tin sinh viÃªn thÃ nh cÃ´ng");'''
text = text.replace(old_return, new_return)
open(path, 'w', encoding='utf-8').write(text)
