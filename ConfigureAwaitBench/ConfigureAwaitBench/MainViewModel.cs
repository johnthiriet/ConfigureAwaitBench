using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ConfigureAwaitBench
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Properties
        public Command Process { get; private set; }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                if (!string.Equals(_message, value))
                {
                    _message = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            Process = new Command(ExecuteProcess);
        }
        #endregion

        #region Process methods
        /// <summary>
        /// Run a long running task composed of small awaited tasks
        /// </summary>
        private async Task<int> ProcessAsync(int loop)
        {
            int result = 0;
            for (int i = 0; i < loop; i++)
            {
                await Task.Delay(10);

                result += i;
            }

            return result;
        }

        /// <summary>
        /// Run a long running task composed of small awaited and configured tasks
        /// </summary>
        private async Task<int> ProcessConfiguredAsync(int loop)
        {
            int result = 0;
            for (int i = 0; i < loop; i++)
            {
                await Task.Delay(10).ConfigureAwait(false);

                result += i;
            }

            return result;
        }

        private async void ExecuteProcess(object parameter)
        {
            try
            {
                // Make sure everything is compiled by doing a cold run
                await ProcessAsync(1);
                await ProcessConfiguredAsync(1);

                // Some values
                const int loop = 1000;
                var sw = new Stopwatch();

                // Tell the user it will take some time
                Message = "Processing...";

                // Start the default process and mesure how long it takes.
                sw.Start();
                await ProcessAsync(loop);
                sw.Stop();

                // Print the duration
                Message = $"ProcessAsync : {sw.ElapsedMilliseconds} ms";

                // Start the configured process and mesure how long it takes.
                sw.Restart();
                await ProcessConfiguredAsync(loop);
                sw.Stop();

                // Print the new duration as well as the old one
                Message = $"{Message}\r\nProcessConfiguredAsync : {sw.ElapsedMilliseconds} ms";
            }
            catch (Exception ex)
            {
                // We are in an async/void method we catch exceptions there.
                Debug.WriteLine(ex);
            }
        }
        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
