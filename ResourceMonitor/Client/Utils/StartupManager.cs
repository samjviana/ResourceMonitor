using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32.TaskScheduler;

namespace Utils {
    class StartupManager
    {
        public static void AddToStartup()
        {
            TaskDefinition taskDefinition = TaskService.Instance.NewTask();
            taskDefinition.RegistrationInfo.Description = "Executa ResourceMonitorClient ao iniciar o sistema";
            taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

            taskDefinition.Triggers.Add(new LogonTrigger());

            taskDefinition.Actions.Add(new ExecAction("\"" + Assembly.GetExecutingAssembly().Location + "\"", null, null));

            string taskName = "ResourceMonitorClient";
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, taskDefinition);
        }

        public static void RemoveFromStartup()
        {
            TaskService taskService = new TaskService();

            taskService.RootFolder.DeleteTask("ResourceMonitorClient");
        }
    }
}
