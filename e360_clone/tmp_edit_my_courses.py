path = r"e360_clone_fe\\Views\\Courses\\My.cshtml"
text = open(path, encoding='utf-8', errors='replace').read()
text = text.replace('<th>Lớp đang dạy</th>', '<th>Lớp đang dạy</th>\n                                <th>Thao tác</th>')
old = '''                                        <td>
                                            <div class="d-flex flex-wrap gap-8">
                                                @foreach (var cls in course.Classes)
                                                {
                                                    <a class="badge bg-primary-100 text-primary-600 text-decoration-none"
                                                       asp-controller="Students"
                                                       asp-action="Index"
                                                       asp-route-classId="@cls.Id">
                                                        @cls.ClassCode
                                                    </a>
                                                }
                                            </div>
                                        </td>'''
new = '''                                        <td>
                                            <span class="badge bg-primary-100 text-primary-600">@course.ClassCode</span>
                                        </td>
                                        <td>
                                            <a class="btn btn-sm btn-primary-600"
                                               asp-controller="Students"
                                               asp-action="Index"
                                               asp-route-classId="@course.ClassId">
                                                Xem
                                            </a>
                                        </td>'''
text = text.replace(old, new)
text = text.replace('colspan="4"', 'colspan="5"')
open(path, 'w', encoding='utf-8').write(text)
