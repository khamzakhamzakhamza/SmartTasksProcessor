# Summary

Hi! This is a real simple package that helps processing a big number of similar tasks (ex. sending email, updating db data etc.). It allows you to specify the number of concurrent processing tasks and the number of retries if the task fails during execution.

# Example

    using  SmartTasksProcessor;
    
    // You need a function that will process a large number of different params.
    var  func = delegate (dynamic  param)
    {
	    try
	    {
		    Console.WriteLine($"Values: {param.Value1} {param.Value2}");
		    
		    var  val = new  Random().Next();
		    if (val % 2 == 0)
			    throw  new  Exception();
		    
		    return  true;
	    }
	    catch
	    {
		    return  false;
	    }
    };
    
    // You need a list of parameters that will be processed concurrently by the function.
    var  paramsList = new  List<int>();
    
    for (var  i =0; i < 20000; i++)
	    paramsList.Add(i);
    
    var  _params = paramsList.Select(i => (dynamic)new { Value1 = i, Value2 = i * 2 }).ToList();
    
    var  processor = new  SmartTaskProcessor(func, _params, threadsCount: 20, retryCount: 3);
    
    processor.Start();
