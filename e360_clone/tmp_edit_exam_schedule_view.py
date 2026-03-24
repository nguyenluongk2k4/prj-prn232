from pathlib import Path
path = Path(r'e360_clone_fe\Views\ExamSchedules\Index.cshtml')
text = path.read_text(encoding='utf-8', errors='replace')
if '@using System.Text.Json' not in text:
    text = text.replace('@model e360_clone_fe.Models.ViewModels.ExamSchedulePageViewModel\n', '@model e360_clone_fe.Models.ViewModels.ExamSchedulePageViewModel\n@using System.Text.Json\n')
text = text.replace('ViewData["Title"] = "Lịch thi";\n    }\n', 'ViewData["Title"] = "Lịch thi";\n    }\n    var useCalendar = ViewData["UseCalendar"] as bool? == true;\n    var listAction = ViewData["ListAction"]?.ToString() ?? "Index";\n')
text = text.replace('var returnUrl = Context.Request.Path + Context.Request.QueryString;\n', 'var returnUrl = Context.Request.Path + Context.Request.QueryString;\n    var calendarEvents = useCalendar\n        ? Model.Slots.SelectMany(slot => slot.Items.Select(item => new\n        {\n            title = $"{item.SubjectCode} - {item.ClassCode}",\n            start = $"{Model.SelectedDate:yyyy-MM-dd}T{slot.StartTime:hh\\:mm}",\n            end = $"{Model.SelectedDate:yyyy-MM-dd}T{slot.EndTime:hh\\:mm}",\n            className = "info",\n            url = Url.Action(listAction, new\n            {\n                date = Model.SelectedDate.ToString("yyyy-MM-dd"),\n                subjectId = item.SubjectId,\n                startTime = slot.StartTime.ToString("hh\\:mm"),\n                endTime = slot.EndTime.ToString("hh\\:mm")\n            })\n        }))\n        : Enumerable.Empty<object>();\n')
if '@section Head' not in text:
    text = text.replace('\n\n<div class="main-content__wrapper">', '\n\n@section Head {\n    @if (useCalendar)\n    {\n        <link rel="stylesheet" href="/assets/css/lib/full-calendar.css" />\n        <link rel="stylesheet" href="/assets/css/lib/calendar.css" />\n    }\n}\n\n<div class="main-content__wrapper">')
path.write_text(text, encoding='utf-8')
