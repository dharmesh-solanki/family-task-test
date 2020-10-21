using Domain.Commands;
using Domain.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebClient.Abstractions;
using Microsoft.AspNetCore.Components;
using Domain.ViewModel;
using Core.Extensions.ModelConversion;

namespace WebClient.Services
{
    public class TaskDataService : ITaskDataService
    {
        private readonly HttpClient httpClient;

        public TaskDataService(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient("FamilyTaskAPI");
            tasks = new List<TaskVm>();
            LoadTasks();
        }

        private IEnumerable<TaskVm> tasks;

        public IEnumerable<TaskVm> Tasks => tasks;

        public TaskVm SelectedTask { get; private set; }


        public event EventHandler TasksUpdated;
        //public event EventHandler TaskSelected;
        public event EventHandler TasksChanged;
        public event EventHandler<string> CreateTaskFailed;
        public event EventHandler<string> CompleteTaskFailed;
        public event EventHandler<string> AssignTaskFailed;

        private async void LoadTasks()
        {
            tasks = (await GetAllTasks()).Payload;
            TasksChanged?.Invoke(this, null);
        }

        private async Task<GetAllTasksQueryResult> GetAllTasks()
        {
            return await httpClient.GetJsonAsync<GetAllTasksQueryResult>("tasks");
        }

        private async Task<CreateTaskCommandResult> Create(CreateTaskCommand command)
        {
            return await httpClient.PostJsonAsync<CreateTaskCommandResult>("tasks", command);
        }

        private async Task<CompleteTaskCommandResult> CompleteTask(CompleteTaskCommand command)
        {
            return await httpClient.PutJsonAsync<CompleteTaskCommandResult>("tasks", command);
        }

        private async Task<AssignTaskCommandResult> AssignTask(AssignTaskCommand command)
        {
            return await httpClient.PostJsonAsync<AssignTaskCommandResult>("tasks/assigntask", command);
        }

        public void SelectTask(Guid id)
        {
            if (tasks.All(taskVm => taskVm.Id != id)) return;
            {
                SelectedTask = tasks.SingleOrDefault(taskVm => taskVm.Id == id);
                TasksChanged?.Invoke(this, null);
            }
        }

        public void ToggleTask(Guid id)
        {
            foreach (var taskModel in Tasks)
            {
                if (taskModel.Id == id)
                {
                    taskModel.IsComplete = !taskModel.IsComplete;
                }
            }

            TasksUpdated?.Invoke(this, null);
        }

        public async Task CreateTask(TaskVm model)
        {
            var result = await Create(model.ToCreateTaskCommand());
            if (result != null)
            {
                var updatedList = (await GetAllTasks()).Payload;

                if (updatedList != null)
                {
                    tasks = updatedList;
                    TasksChanged?.Invoke(this, null);
                    return;
                }
                CreateTaskFailed?.Invoke(this, "The creation was successful, but we can no longer get an updated list of tasks from the server.");
            }

            CreateTaskFailed?.Invoke(this, "Unable to create record.");
        }

        public async void CompleteTask(Guid id)
        {
            var result = await CompleteTask(new CompleteTaskCommand() { TaskId = id });
            if (result.Succeed)
            {
                var updatedList = (await GetAllTasks()).Payload;
                tasks = updatedList;
                TasksChanged?.Invoke(this, null);
            }
            else
            {
                CompleteTaskFailed?.Invoke(this, "Unable to complete task.");
            }
        }

        public async void AssignTask(Guid memberId)
        {
            if (memberId != Guid.Empty && SelectedTask != null)
            {
                var result = await AssignTask(new AssignTaskCommand() { MemberId = memberId, TaskId = SelectedTask.Id });

                // empty task selection
                SelectedTask = null;

                if (result.Succeed)
                {
                    var updatedList = (await GetAllTasks()).Payload;
                    tasks = updatedList;
                    TasksChanged?.Invoke(this, null);
                }
                else
                {
                    AssignTaskFailed?.Invoke(this, "Unable to assign task.");
                }
            }
            else
            {
                AssignTaskFailed?.Invoke(this, "MemberId and TaskId are require to assign task.");
            }
        }

    }
}