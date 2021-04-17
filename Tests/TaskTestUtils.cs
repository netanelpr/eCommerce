﻿using System;
using System.Threading.Tasks;

namespace Tests
{
    public class TaskTestUtils
    {
        public static Task<T[]> CreateAndRunTasks<T>(Func<T> func, int numberOfTasks)
        {
            Task<T>[] tasks = CreateArrayOfTasks(func, numberOfTasks);
            RunTasks(tasks);

            return Task.WhenAll<T>(tasks);
        }

        public static Task<T>[] CreateArrayOfTasks<T>(Func<T> func, int numberOfTasks)
        {
            Task<T>[] tasks = new Task<T>[numberOfTasks];
            for (var i = 0; i < numberOfTasks; i++)
            {
                tasks[i] = new Task<T>(func);
            }

            return tasks;
        }
        
        public static void RunTasks<T>(Task<T>[] tasks)
        {
            foreach (var task in tasks)
            {
                task.Start();
            }
        }
    }
}