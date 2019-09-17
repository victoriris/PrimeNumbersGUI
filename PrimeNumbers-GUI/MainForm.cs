using System;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeNumbers_GUI
{
    public partial class MainForm : Form
    {
        private CancellationTokenSource cancellationTokenSource;
        private int lastProgressValue;
        private int lastNum;
        private bool has_ended = false;

        public MainForm()
        {
            InitializeComponent();
        }
        
        private async void startButton_Click(object sender, EventArgs e)
        {
            // Find all prime numbers starting between the first and last numbers
            int firstNum;

            try
            {
                firstNum = Convert.ToInt32(startNumTextBox.Text);
                lastNum = Convert.ToInt32(endNumTextBox.Text);
            } catch (FormatException)
            {
                MessageBox.Show("Invalid Input", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            numbersTextBox.Clear();
            
            // Prevent user from messing with certain controls while job is running
            progressBar1.Minimum = firstNum;
            progressBar1.Maximum = lastNum;
            progressBar1.Visible = true;
            cancelButton.Enabled = true;
            pauseButton.Enabled = true;
            startNumTextBox.Enabled = false;
            startButton.Enabled = false;
            endNumTextBox.Enabled = false;


            UseWaitCursor = true;

            // Task
            Task getPrimeTask = getPrimes(firstNum, lastNum);
            await getPrimeTask;


            // Let the user know we did something even if no prime nums were found
            if (numbersTextBox.TextLength == 0)
            {
                numbersTextBox.Text = "None.";
            }


            UseWaitCursor = false;

            if (has_ended)
            {
                resetForm();
            }

            
        }

        private void resetForm ()
        {
            // Reset the form
            has_ended = false;
            pauseButton.Text = "Pause";
            startNumTextBox.Enabled = true;
            endNumTextBox.Enabled = true;
            progressBar1.Value = progressBar1.Minimum;
            progressBar1.Visible = false;
            cancelButton.Enabled = false;
            startButton.Enabled = true;
            pauseButton.Enabled = false;
        }

        private async Task getPrimes(int firstNum, int lastNum)
        {

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            await Task.Run(() => {
                for (int i = firstNum; i <= lastNum; i++)
                {
                    lastProgressValue = i;
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    // See which numbers are factors and append them to the numbers text box
                    if (IsPrime(i))
                    {
                        AddNumberToTextBox(i);
                    }
                }
                if (lastProgressValue == lastNum)
                {
                    has_ended = true;
                }
            });

        }

        private bool IsPrime(int num)
        {
            if (num < 2)
                return false;

            // Look for a number that evenly divides the num
            for (int i = 2; i <= num / 2; i++)
                if (num % i == 0)
                    return false;

            // No divisors means the number is prime
            return true;
        }

        private void AddNumberToTextBox(int num)
        {
            Invoke((Action)delegate ()
           {
               numbersTextBox.AppendText(num.ToString());
               numbersTextBox.AppendText(Environment.NewLine);
               progressBar1.Value = num;
           });
            
        }

        private async void pauseButton_Click(object sender, EventArgs e)
        {
            if (pauseButton.Text == "Pause")
            {
                // Pause the current job 
                cancellationTokenSource.Cancel();
                pauseButton.Text = "Resume";
            }
            else
            {
                // Resume the current job 
                pauseButton.Text = "Pause";
                await getPrimes(lastProgressValue, lastNum);
                if (lastProgressValue == lastNum)
                {
                    resetForm();
                }
            }
           
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // Cancel the work done in the for loop
            has_ended = true;
            cancellationTokenSource.Cancel();
            resetForm();
            pauseButton.Enabled = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }
    }
}
