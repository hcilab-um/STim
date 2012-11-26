using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using STimWPF.Interaction;
using STimWPF.Util;

namespace STimWPF.Interaction
{
	public class GestureRecognizer : INotifyPropertyChanged
	{

		//Lateral movement is given in percentage of the layout, from [0..1)
		private const double LATERAL_THRESHOLD_TIMER = 0.01;

		private Point3D lastPos = new Point3D(0, 0, 0);
		private StringBuilder movementSequenceSB = new StringBuilder();
		private List<InputInfo> movementSequence = new List<InputInfo>(120);

		//Variables for the SelectionMethod.Timer selection
		private static readonly String REGEX_START_TIMER = "(M|S){15}";
		private static readonly String TAP_REGEX_TIMER = "(M|S){25}";
		private Regex regexStartTimer = new Regex(REGEX_START_TIMER);
		private Regex regexTapTimer = new Regex(TAP_REGEX_TIMER);

		private TimerState timerState = TimerState.Nothing;

		public String MovementSequence
		{
			get { return movementSequenceSB.ToString(); }
		}

		public TimerState TimerState
		{
			get { return timerState; }
			set
			{
				if (timerState == value)
					return;
				timerState = value;
				OnPropertyChanged("TimerState");
			}
		}

		internal ICollection<InteractionGesture> ProcessGestures(Skeleton skeleton, double deltaTimeMilliseconds, Point3D cursor, Point3D secondaryCursor,
			SelectionMethod selectionM, bool userClicked)
		{
			List<InteractionGesture> gestures = null;

			if (selectionM == SelectionMethod.Timer)
				gestures = ProcessGesturesTimer(cursor, deltaTimeMilliseconds);
			else if (selectionM == SelectionMethod.Click)
				gestures = ProcessGesturesClick(cursor, userClicked);
			return gestures;
		}

		private List<InteractionGesture> ProcessGesturesTimer(Point3D cursor, double delta)
		{
			if (cursor.X == -1 || cursor.Y == -1)
				return null;

			List<InteractionGesture> gestures = new List<InteractionGesture>();
			InputInfo info = new InputInfo() { Position = cursor };
			System.Windows.Media.Media3D.Vector3D displacement = cursor - lastPos;
			lastPos = cursor;

			if (movementSequenceSB.Length == movementSequence.Capacity)
			{
				movementSequenceSB.Remove(0, 1);
				movementSequence.RemoveAt(0);
			}

			if (Math.Abs(displacement.X) > LATERAL_THRESHOLD_TIMER || Math.Abs(displacement.Y) > LATERAL_THRESHOLD_TIMER)
				info.Movement = 'M'; //moving
			else
				info.Movement = 'S'; //steady --- not moving
			movementSequenceSB.Append(info.Movement);
			movementSequence.Add(info);

			String tmpSequence = MovementSequence;
			if (timerState == STimWPF.Interaction.TimerState.Nothing)
			{
				MatchCollection starts = regexStartTimer.Matches(tmpSequence);
				if (starts.Count > 0)
				{
					Match start = starts[0];
					TimerState = STimWPF.Interaction.TimerState.Running;
					movementSequenceSB.Remove(start.Index, start.Length);
					movementSequence.RemoveRange(start.Index, start.Length);
				}
			}
			else if (timerState == STimWPF.Interaction.TimerState.Running)
			{
				MatchCollection selections = regexTapTimer.Matches(tmpSequence);
				if (selections.Count > 0)
				{
					Match selection = selections[0];
					gestures.Add(new InteractionGesture()
					{
						Time = DateTime.Now,
						Type = GestureType.Tap,
						Position = movementSequence[selection.Index].Position
					});
					TimerState = STimWPF.Interaction.TimerState.Nothing;
					movementSequenceSB.Remove(selection.Index, selection.Length);
					movementSequence.RemoveRange(selection.Index, selection.Length);
				}
			}
			OnPropertyChanged("MovementSequence");

			return gestures;
		}

		private List<InteractionGesture> ProcessGesturesClick(Point3D cursor, bool userClicked)
		{
			if (cursor.X == -1 || cursor.Y == -1)
				return null;

			List<InteractionGesture> gestures = new List<InteractionGesture>();
			if (!userClicked)
				return gestures;

			gestures.Add(new InteractionGesture()
			{
				Position = cursor,
				Type = GestureType.Tap,
				Time = DateTime.Now
			});
			return gestures;
		}

		internal void InteractionCtrngine_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if ("SelectionMethod".Equals(e.PropertyName))
			{
				movementSequenceSB.Clear();
				movementSequence.Clear();
				TimerState = STimWPF.Interaction.TimerState.Nothing;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private class InputInfo
		{
			public Point3D Position { get; set; }
			public char Movement { get; set; }
		}

	}
}
