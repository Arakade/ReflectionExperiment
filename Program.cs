#define DO_PARENTS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectionExperiment {
	class Program {
		static void Main(string[] args) {
			Log.log("Starting...");
			var tasks = new Task[] {
				new Task("simple Task"),
				new LeafWithout("leaf without"),
				new LeafWithFixed("leaf with"),
				new ParentTask("Parent with none") { },
				new ParentTask("Parent without") {
					Children = new Task[] { new LeafWithout("child without") }.ToList()
				},
				new ParentTask("Parent with") {
					Children = new Task[] { new LeafWithFixed("child with") }.ToList()
				},
			};

			for (int i = 0; i < tasks.Length; i++) {
				var containsFixed = TaskContainsMethod("OnFixedUpdate", tasks[i]);
				Log.log("{0} {1}", tasks[i], containsFixed);
			}

			Log.log("Done.");
		}


		private static bool TaskContainsMethod(string methodName, Task task) {
			if (task == null) {
				return false;
			}

#if !UNITY_EDITOR && (UNITY_WSA_8_0 || UNITY_WSA_8_1)
            var method = task.GetType().GetMethod(methodName, System.BindingFlags.Public |System.BindingFlags.NonPublic | System.BindingFlags.Instance);
#else
			var method = task.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
#endif
			if (method != null && !typeof(Task).Equals(method.DeclaringType)) {
				return true;
			}

#if DO_PARENTS
			if (task is ParentTask) {
				var parentTask = task as ParentTask;
				if (parentTask.Children != null) {
					for (int i = 0; i < parentTask.Children.Count; ++i) {
						if (TaskContainsMethod(methodName, parentTask.Children[i])) {
							return true;
						}
					}
				}
			}
#endif
			return false;
		}
	}

	class Action {
		public readonly string name;

		public Action(string name) {
			this.name = name;
		}

		public override string ToString() {
			return string.Format("{0}-{1}(name:\"{2}\")", base.ToString(), GetType(), name);
		}
	}

	class Task : Action {
		public Task(string name) : base(name) { }

		public virtual void OnFixedUpdate() {

		}

		public virtual void OnUpdate() {

		}
	}

	class ParentTask : Task {
		public ParentTask(string name) : base(name) { }

		public List<Task> Children = new List<Task>();

		public override string ToString() {
			var childrenToString = string.Join(", ", Children);
			return string.Format("{0}-Children:[{1}]", base.ToString(), childrenToString);
		}
	}

	class LeafWithout : Task {
		public LeafWithout(string name) : base(name) { }
	}

	class LeafWithFixed : Task {
		public LeafWithFixed(string name) : base(name) { }

		public override void OnFixedUpdate() {
			base.OnFixedUpdate();
			Log.log("{0}", this);
		}
	}

	static class Log {
		public static void log(string fmt, params object[] args) {
			System.Diagnostics.Debug.WriteLine(fmt, args);
		}
	}
}
