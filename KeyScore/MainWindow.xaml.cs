using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KeyScore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        private string targetCharacters = "";
        private int currentIndex = 0;
        private Stopwatch stopwatch = new Stopwatch();
        private string highscoreFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KeyScore", "highscores.txt");

        public MainWindow()
        {
            InitializeComponent();
            LoadHighScores();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void StartGame()
        {
            // Generate 20 random characters
            targetCharacters = new string(Enumerable.Range(0, 20)
                                      .Select(i => random.Next(0, 36))
                                      .Select(x => x < 26 ? (char)(x + 'a') : (char)(x - 26 + '0'))
                                      .ToArray());

            currentIndex = 0;
            CurrentChar.Text = targetCharacters[currentIndex].ToString();

            UserInput.Clear();
            UserInput.IsEnabled = true;
            UserInput.Focus();

            stopwatch.Restart();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back) return; // Ignore backspace

            char typedChar = (char)KeyInterop.VirtualKeyFromKey(e.Key);

            if (char.ToLower(typedChar) == targetCharacters[currentIndex])
            {
                // Correct key pressed
                FlashScreen(Colors.Green);

                currentIndex++;
                if (currentIndex < targetCharacters.Length)
                {
                    CurrentChar.Text = targetCharacters[currentIndex].ToString();
                }
                else
                {
                    // Game over
                    stopwatch.Stop();
                    EndGame();
                }
            }
            else
            {
                // Wrong key pressed
                FlashScreen(Colors.Red);
            }

            e.Handled = true;
            UserInput.Clear();
        }

        private void FlashScreen(Color color)
        {
            this.Background = new SolidColorBrush(color);
            var timer = new System.Timers.Timer(200);
            timer.Elapsed += (s, e) =>
            {
                this.Dispatcher.Invoke(() => { this.Background = Brushes.White; });
                timer.Stop();
            };
            timer.Start();
        }

        private void EndGame()
        {
            UserInput.IsEnabled = false;
            TimeSpan timeTaken = stopwatch.Elapsed;

            // Ask for name for high score
            string name = Microsoft.VisualBasic.Interaction.InputBox("Enter your name for the high score:", "High Score");

            // Save high score
            SaveHighScore(name, timeTaken);

            // Load and sort high scores to find the user's placement
            var scores = File.ReadAllLines(highscoreFile)
                .Select(line => line.Split('|'))
                .Select(parts => new
                {
                    Name = parts[0],
                    Time = double.Parse(parts[1]) // time in milliseconds
                })
                .OrderBy(score => score.Time) // Sort by time
                .ToList();

            // Find the user's placement
            int placement = scores.FindIndex(s => s.Name == name && s.Time == timeTaken.TotalMilliseconds) + 1;

            // Show the user their placement
            MessageBox.Show($"Your time: {timeTaken:mm\\:ss\\:fff}\nYou placed #{placement}!", "Game Over");

            // Reload high scores display
            LoadHighScores();
        }

        private void SaveHighScore(string name, TimeSpan time)
        {
            string directory = System.IO.Path.GetDirectoryName(highscoreFile);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save the high score in the format "name|time"
            string scoreEntry = $"{name}|{time.TotalMilliseconds}";
            File.AppendAllText(highscoreFile, scoreEntry + Environment.NewLine);
        }

        private void LoadHighScores()
        {
            if (File.Exists(highscoreFile))
            {
                var scores = File.ReadAllLines(highscoreFile)
                    .Select(line => line.Split('|'))
                    .Select(parts => new
                    {
                        Name = parts[0],
                        Time = double.Parse(parts[1]) // time in milliseconds
                    })
                    .OrderBy(score => score.Time) // Sort by time
                    .ToList();

                // Display the high scores
                HighScores.Text = string.Join(Environment.NewLine,
                    scores.Select((score, index) => $"{index + 1}. {score.Name} - {TimeSpan.FromMilliseconds(score.Time):mm\\:ss\\:fff}"));
            }
            else
            {
                HighScores.Text = "No high scores yet!";
            }
        }
    }
}