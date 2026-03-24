path = r"e360_clone_fe\\Views\\Students\\Details.cshtml"
text = open(path, encoding='utf-8', errors='replace').read()
old = ': $"Lớp {Model.ClassId}";'
new = ': $"Lớp {Model.ClassId}";\n    var avatarSrc = !string.IsNullOrWhiteSpace(Model.AvatarUrl)\n        ? Model.AvatarUrl\n        : "/assets/images/thumbs/student-details-img.png";'
text = text.replace(old, new)
open(path, 'w', encoding='utf-8').write(text)
