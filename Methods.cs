using System.Globalization;
using HtmlAgilityPack;
using ExcelDataReader;
using System.Text.RegularExpressions;
using System.Text;
using System.Data;
using ScheduleBotik;
public static class Methods
{
    static string oldUrl;
    public static bool isChangeUrl;
    public static List<Lesson> Todaylessons;
    public static async Task DownloadHtml(string s = "https://perm.hse.ru/students/timetable")
    {
        string domain = "https://perm.hse.ru";
        try
        {
            using var client = new HttpClient();
            var html = await client.GetStringAsync(s);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            string newFileName = $"Расписание занятий (неделя №";

            HtmlNode linkNode = doc.DocumentNode.SelectSingleNode($"//a[contains(text(), '{newFileName}')]");
            if (linkNode != null)
            {
                string fileUrl = linkNode.GetAttributeValue("href", "");
                if (fileUrl.StartsWith("/"))
                {
                    fileUrl = domain + fileUrl;
                }
                string oldUrl = File.ReadAllText("last_url.txt");
                if (oldUrl == fileUrl)
                {
                    isChangeUrl = false;
                    return;
                }
                else
                {
                    isChangeUrl = true;
                    File.WriteAllText("last_url.txt", fileUrl);
                }
                byte[] fileBytes = await client.GetByteArrayAsync(fileUrl);
                string filePath = Path.Combine(Environment.CurrentDirectory, "Schedule.xls");
                await File.WriteAllBytesAsync(filePath, fileBytes);

                //Для отладки
                Console.WriteLine("Файл успешно загружен");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static bool CheckGroupName(string group)
    {
        Regex regex = new Regex(@"^[а-яА-Я]{2,3}-\d{2}-\d{1}$");
        return regex.IsMatch(group);
    }

    static int GetGroupColumn(System.Data.DataTable table, string group)
    {
        for (int j = 0; j < table.Columns.Count; j++)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (CheckGroupName(table.Rows[i][j].ToString()))
                {
                    while (group != table.Rows[i][j].ToString())
                    {
                        j++;
                    };
                    return j;
                }
            }
        }
        throw new Exception("Такой группы нет");
        return 0;
    }

    static int[] GetDateRows(System.Data.DataTable table, string date)
    {
        int[] rows = new int[2];
        rows[0] = 1000;
        int i = 0;
        int countOfEmptyCells = 0;
        do
        {
            if (table.Rows[i][0].ToString().Contains(date))
            {
                rows[0] = i;
                i++;
                countOfEmptyCells = 0;
                continue;
            }
            if (rows[0] != 1000 && table.Rows[i][0].ToString() != "")
            {
                rows[1] = i;
                return rows;
            }
            if (countOfEmptyCells > 40)
            {
                rows[1] = i;
                break;
            }
            countOfEmptyCells++;
            i++;
        } while (i < 999);
        return rows;
    }
    static List<CellRange> mergedCells = new();
    static string GetStringFromCell(DataTable table, int row, int column)
    {
        var cellArea = mergedCells.FirstOrDefault(x =>
        row >= x.FromRow && row <= x.ToRow &&
        column >= x.FromColumn && column <= x.ToColumn);

        if (cellArea != null)
        {
            return table.Rows[cellArea.FromRow][cellArea.FromColumn].ToString();
        }
        else
        {
            return table.Rows[row][column].ToString();
        }
    }
    public static string GetSchedulePerDay(string group, int kursNumber, DateTime date, bool isNotUser = false)
    {
        if (!CheckGroupName(group))
        {
            throw new Exception("Введеная Вами группа не существует");
        }
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string SchedulePath = Path.Combine(Environment.CurrentDirectory, "Schedule.xls");
        using (var stream = File.Open(SchedulePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                mergedCells = reader.MergeCells.ToList();
                var result = reader.AsDataSet();
                System.Data.DataTable table = result.Tables[kursNumber - 1];
                int groupColumn = GetGroupColumn(table, group);
                int[] dateDiapason = GetDateRows(table, date.ToString("dd.MM.yyyy"));
                StringBuilder stringBuilder = new StringBuilder();
                string firstLine = $"Расписание заняти на {date.ToString("dd.MM.yyyy")}";
                stringBuilder.AppendLine(firstLine);
                int countLessons = 0;
                for (int i = dateDiapason[0]; i < dateDiapason[1]; i++)
                {
                    
                    string lessonName = GetStringFromCell(table, i, groupColumn).Replace("\n","");
                    if (lessonName.Contains("Английск"))
                    {
                        continue;
                    }
                    countLessons++;
                    if (!string.IsNullOrEmpty(lessonName))
                    {
                        var timeLessons = table.Rows[i][1].ToString().Split('-');
                        Todaylessons.Add(new Lesson(lessonName, TimeSpan.Parse(timeLessons[0].Substring(3)), TimeSpan.Parse(timeLessons[1])));
                        stringBuilder.AppendLine($"{countLessons}-я пара - {lessonName} с {timeLessons[0].Substring(3)} до {timeLessons[1]};");
                    }
                }
                if (isNotUser)
                {
                    return "";
                }
                if (stringBuilder.ToString() == firstLine)
                {
                    return "Пар нет";
                }
                return stringBuilder.ToString();
            }
        }
    }

    public static string GetNearlyLesson()
    {

        var currentTime = DateTime.Now.TimeOfDay;

        var nextLesson = Todaylessons.FirstOrDefault(x => x.EndTime > currentTime);

        if (nextLesson == null)
        {
            return "На сегодня пар больше нет. Можете выдохнуть";
        }
        else
        if (nextLesson.BeginTime > currentTime)
        {
            var TimeToNextLesson = nextLesson.BeginTime - currentTime;
            return $"Ближайшая пара {nextLesson.Name}\n" +
                $"начнется через {TimeToNextLesson.Hours} ч. {TimeToNextLesson.Minutes} мин.";
        }
        else
        {
            var TimeToEndLesson = nextLesson.EndTime - currentTime;
            return $"Сейчас идет пара {nextLesson.Name}\n" +
                $"Она закончиться через {TimeToEndLesson.Hours} ч. {TimeToEndLesson.Minutes} мин.";
        }
    }


}