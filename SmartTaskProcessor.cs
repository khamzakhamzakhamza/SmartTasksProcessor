namespace SmartTasksProcessor
{
	public class SmartTaskProcessor
	{
		public Func<dynamic, bool> Action { get; private set; }

		public List<dynamic> Params { get; private set; }

		public int RetryCount { get; private set; }

		public int ThreadsCount { get; private set; }

		public List<int> Errors { get; set; } = new List<int>();


		private volatile bool _finished;

		private volatile int _counter;

		private readonly object _counterLock = new object();

		public SmartTaskProcessor(Func<dynamic, bool> action, List<dynamic> _params, int threadsCount = 10, int retryCount = 2)
        {
			Action = action;
			Params = _params;
			ThreadsCount = threadsCount;
			RetryCount = retryCount;

			if (_params.Count() < threadsCount)
				throw new ArgumentException("Threads count must be less than params count");
        }

		public void Start()
        {
			for (var i = 0; i < ThreadsCount; i++)
            {
				int clone = i;
				Task.Run(() => StartTask(clone));
            }

			while (!_finished)
				Thread.Sleep(1000);
		}

		private void FinishTask(bool result, int paramIndex, int attempt)
		{
			int counter = _counter;

			if (result)
			{
				Console.WriteLine($"counter: {counter} paramIndex: {paramIndex} task finished successfully");
			}
			else
			{
				Console.WriteLine($"counter: {counter} paramIndex: {paramIndex} task failed");

				if (attempt < RetryCount)
				{
					StartTask(paramIndex, attempt);
					
					return;
				}
				else
				{
					Errors.Add(paramIndex);
				}
			}

			counter = Interlocked.Increment(ref _counter);

			var nextParamIndex = ThreadsCount + counter - 1;

			// Making sure there are tasks left to work on
			if (nextParamIndex < Params.Count())
				StartTask(nextParamIndex);

			if (counter == Params.Count())
				_finished = true;
		}

		private void StartTask(int paramIndex, int attempt = 0)
		{
			var result = false;

			try
			{
				result = Action(Params[paramIndex]);
			}
			finally
			{
				FinishTask(result, paramIndex, ++attempt);
			}
		}
	}
}
