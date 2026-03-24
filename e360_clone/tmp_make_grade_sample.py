import json, csv, random, sys
from urllib.request import urlopen, Request

url = 'http://localhost:5104/api/students/by-subject?subjectId=37&classId=13&pageNumber=1&pageSize=2000'
try:
    with urlopen(Request(url)) as r:
        data = json.load(r)
except Exception as e:
    print('ERROR_FETCH', e)
    sys.exit(1)

items = data.get('data') or []
if not items:
    print('NO_STUDENTS')
    sys.exit(1)

rows = []
for s in items:
    code = s.get('studentCode') or s.get('StudentCode')
    if not code:
        continue
    score = round(random.uniform(4.0, 9.8), 1)
    letter = 'A' if score>=8.5 else 'B' if score>=7.0 else 'C' if score>=5.5 else 'D'
    rows.append([code, score, letter, 'Sample'])

csv_path = r'C:\Git\prj-prn232\e360_clone\docs\sample_grades_subject37_class13.csv'
with open(csv_path, 'w', newline='', encoding='utf-8') as f:
    w = csv.writer(f)
    w.writerow(['StudentCode','Score','LetterGrade','Notes'])
    w.writerows(rows)

print('CSV_OK', csv_path, len(rows))

try:
    import openpyxl
    wb = openpyxl.Workbook()
    ws = wb.active
    ws.title = 'Grades'
    ws.append(['StudentCode','Score','LetterGrade','Notes'])
    for r in rows:
        ws.append(r)
    xlsx_path = r'C:\Git\prj-prn232\e360_clone\docs\sample_grades_subject37_class13.xlsx'
    wb.save(xlsx_path)
    print('XLSX_OK', xlsx_path)
except Exception as e:
    print('XLSX_FAIL', e)
