using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KvizAlkalmazas
{

    //ha más kérdéseket akarunk akkor a megadott format alapján kell kicserélni a kérdéseleket a kerdesek.txt fájlban
    public partial class MainWindow : Window
    {
        private List<Question> allQuestions;
        private List<Question> selectedQuestions;
        private int currentQuestionIndex = 0;
        private int correctAnswers = 0;
        private string userName;

        public MainWindow()
        {
            InitializeComponent();
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            allQuestions = new List<Question>();
            string filePath = "kerdesek.txt";

            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("A kerdesek.txt fájl nem található!\n\n" +
                                  "Hozz létre egy kerdesek.txt fájlt a program mappájában.\n" +
                                  "Formátum: Kérdés|Válasz1|Válasz2|Válasz3|Válasz4|HelyesVálaszIndexe",
                        "Hiányzó fájl", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                string[] lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('|');
                    if (parts.Length == 6)
                    {
                        Question q = new Question
                        {
                            QuestionText = parts[0].Trim(),
                            Options = new List<string>
                            {
                                parts[1].Trim(),
                                parts[2].Trim(),
                                parts[3].Trim(),
                                parts[4].Trim()
                            },
                            CorrectAnswerIndex = int.Parse(parts[5].Trim())
                        };
                        allQuestions.Add(q);
                    }
                }

                if (allQuestions.Count < 10)
                {
                    MessageBox.Show($"A kerdesek.txt fájl csak {allQuestions.Count} kérdést tartalmaz.\n" +
                                  "Minimum 10 kérdés szükséges a kvíz indításához!",
                        "Kevés kérdés", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba a fájl beolvasásakor:\n{ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

      
        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            NameInputDialog nameDialog = new NameInputDialog();
            if (nameDialog.ShowDialog() == true)
            {
                userName = nameDialog.UserName;
            }
            else
            {
               //nincs user
                return;
            }

            
            if (allQuestions.Count < 10)
            {
                MessageBox.Show("Legalább 10 kérdés szükséges a kvíz indításához!",
                    "Figyelem", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            SelectRandomQuestions();

            WelcomePanel.Visibility = Visibility.Collapsed;
            QuizPanel.Visibility = Visibility.Visible;
            ResultPanel.Visibility = Visibility.Collapsed;

          
            currentQuestionIndex = 0;
            correctAnswers = 0;         
           ShowQuestion();
        }

      
        private void SelectRandomQuestions()
        {
            Random random = new Random();
            selectedQuestions = allQuestions
                .OrderBy(x => random.Next())
                .Take(10)
                .ToList();
        }

     
        private void ShowQuestion()
        {
            if (currentQuestionIndex < selectedQuestions.Count)
            {
                var q = selectedQuestions[currentQuestionIndex];

                QuestionText.Text = q.QuestionText;
                QuestionCounter.Text = $"Kérdés: {currentQuestionIndex + 1}/10";

                QuestionProgress.Value = currentQuestionIndex + 1;

                Answer1.Content = q.Options[0];
                Answer2.Content = q.Options[1];
                Answer3.Content = q.Options[2];
                Answer4.Content = q.Options[3];

                Answer1.IsChecked = false;
                Answer2.IsChecked = false;
                Answer3.IsChecked = false;
                Answer4.IsChecked = false;

                if (currentQuestionIndex == selectedQuestions.Count - 1)
                {
                    SubmitAnswerButton.Content = "Befejezés";
                }
                else
                {
                    SubmitAnswerButton.Content = "Válasz rögzítése";
                }
            }
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = GetSelectedAnswerIndex();

            if (selectedIndex == -1)
            {
                MessageBox.Show("Kérlek válassz egy választ!", "Figyelem",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var q = selectedQuestions[currentQuestionIndex];
            if (selectedIndex == q.CorrectAnswerIndex)
            {
                correctAnswers++;
            }

            currentQuestionIndex++;

            if (currentQuestionIndex < selectedQuestions.Count)
            {
                ShowQuestion();
            }
            else
            {
           
                ShowResults();
                SaveResults();
            }
        }

        private int GetSelectedAnswerIndex()
        {
            if (Answer1.IsChecked == true) return 0;
            if (Answer2.IsChecked == true) return 1;
            if (Answer3.IsChecked == true) return 2;
            if (Answer4.IsChecked == true) return 3;
            return -1; //ha nincs valasz
        }

        private void ShowResults()
        {
            QuizPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Visible;
            WelcomePanel.Visibility = Visibility.Collapsed;

            int percentage = (correctAnswers * 100) / selectedQuestions.Count;
            string message;

            if (percentage >= 90)
                message = "Kiválóan teljesítettél! 🎉";
            else if (percentage >= 70)
                message = "Jó munka! 👍";
            else if (percentage >= 50)
                message = "Közepes eredmény. Gyakorolj még! 📚";
            else
                message = "Gyakorolj még sokat! 💪";

            ResultText.Text = $"Név: {userName}\n\n" +
                              $"Pontszámod: {correctAnswers}/{selectedQuestions.Count}\n" +
                              $"Százalék: {percentage}%\n\n" +
                              message + "\n\n" +
                              "Az eredmény automatikusan mentésre került.";
        }


        private void SaveResults()
        {
            try
            {
                string result = $"{userName}:{correctAnswers}";
                string filePath = "eredmenyek.txt";

                File.AppendAllText(filePath, result + Environment.NewLine, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba az eredmény mentése során:\n{ex.Message}",
                    "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestartQuiz_Click(object sender, RoutedEventArgs e)
        {
            
            WelcomePanel.Visibility = Visibility.Visible;
            QuizPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Collapsed;

            currentQuestionIndex = 0;
            correctAnswers = 0;
            userName = string.Empty;
        }

     
        public class Question
        {
            public string QuestionText { get; set; }
            public List<string> Options { get; set; }
            public int CorrectAnswerIndex { get; set; }
        }
    }

  
    public class NameInputDialog : Window
    {
        private TextBox nameTextBox;
        public string UserName { get; private set; }

        public NameInputDialog()
        {
            Title = "Név megadása";
            Width = 400;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E0E2DB"));

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = "Kérlek add meg a teljes neved:",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6AF2E"))
            };
            Grid.SetRow(titleBlock, 0);
            grid.Children.Add(titleBlock);

            nameTextBox = new TextBox
            {
                FontSize = 14,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(nameTextBox, 1);
            grid.Children.Add(nameTextBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 100,
                Height = 40,
                Margin = new Thickness(5),
                FontSize = 14,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6AF2E")),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Content = "Mégse",
                Width = 100,
                Height = 40,
                Margin = new Thickness(5),
                FontSize = 14,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#191716")),
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 3);
            grid.Children.Add(buttonPanel);

            Content = grid;

            nameTextBox.Focus();
            nameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    OkButton_Click(null, null);
                }
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UserName = nameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(UserName))
            {
                MessageBox.Show("Kérlek add meg a neved!", "Figyelem",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
