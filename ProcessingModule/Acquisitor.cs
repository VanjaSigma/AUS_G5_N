using Common;
using System;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for periodic polling.
    /// </summary>
    public class Acquisitor : IDisposable
	{
		private AutoResetEvent acquisitionTrigger;
        private IProcessingManager processingManager;
        private Thread acquisitionWorker;
		private IStateUpdater stateUpdater;
		private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Acquisitor"/> class.
        /// </summary>
        /// <param name="acquisitionTrigger">The acquisition trigger.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="stateUpdater">The state updater.</param>
        /// <param name="configuration">The configuration.</param>
		public Acquisitor(AutoResetEvent acquisitionTrigger, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration)
		{
			this.stateUpdater = stateUpdater;
			this.acquisitionTrigger = acquisitionTrigger;
			this.processingManager = processingManager;
			this.configuration = configuration;
			this.InitializeAcquisitionThread();
			this.StartAcquisitionThread();
		}

		#region Private Methods

        /// <summary>
        /// Initializes the acquisition thread.
        /// </summary>
		private void InitializeAcquisitionThread()
		{
			this.acquisitionWorker = new Thread(Acquisition_DoWork);
			this.acquisitionWorker.Name = "Acquisition thread";
		}

        /// <summary>
        /// Starts the acquisition thread.
        /// </summary>
		private void StartAcquisitionThread()
		{
			acquisitionWorker.Start();
		}

        /// <summary>
        /// Acquisitor thread logic.
        /// </summary>
		private void Acquisition_DoWork()
        {
            ushort counter = 0;

            while (true)
            {
                acquisitionTrigger.WaitOne();
                counter++;

                foreach (IConfigItem item in configuration.GetConfigurationItems())
                {
                    // analogne (2100 i 2500) - svake 4 sekunde
                    if ((item.StartAddress == 2100 || item.StartAddress == 2500) && counter % 4 == 0)
                    {
                        processingManager.ExecuteReadCommand(
                            item,
                            counter,
                            91,
                            item.StartAddress,
                            item.NumberOfRegisters
                        );
                    }
                    // digitalne (2200, 2202, 2205, 2206) - svake 2 sekunde
                    else if ((item.StartAddress == 2200 || item.StartAddress == 2202 || item.StartAddress == 2205 || item.StartAddress == 2206) && counter % 2 == 0)
                    {
                        processingManager.ExecuteReadCommand(
                            item,
                            counter,
                            91,
                            item.StartAddress,
                            item.NumberOfRegisters
                        );
                    }
                }
            }
        }

        #endregion Private Methods

        /// <inheritdoc />
        public void Dispose()
		{
			acquisitionWorker.Abort();
        }
	}
}