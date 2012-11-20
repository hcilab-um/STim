using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using STimWPF.Pointing;

namespace STimWPF
{
	/// <summary>
	/// Interaction logic for TextEntryWindow.xaml
	/// </summary>
	public partial class TextEntryWindow : Window
	{
		private Random randon = new Random((int)DateTime.Now.Ticks);

		public App AppInstance { get; set; }
		
		public Core CoreInstance
		{
			get { return Core.Instance; }
		}

		public TextEntryWindow(App appInst)
		{
			AppInstance = appInst;
			InitializeComponent();
			CoreInstance.InteractionCtr.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(InteractionCtr_PropertyChanged);
		}

		private void textEntryW_Closed(object sender, EventArgs e)
		{
			AppInstance.CloseApp(this);
		}

		public void Reset()
		{
			tbFeedback.Text = "";
			tbFeedback.Background = Brushes.Transparent;
		}

		private string GenerateRandomBoxSequence()
		{
			StringBuilder rdnSequence = new StringBuilder();
			List<Key> unusedKeys = new List<Key>(BoxInteractionMethod.layout);
			RemoveKeys(unusedKeys);
			List<Key> randomOrder = new List<Key>();
			Key randomKey;
			while (unusedKeys.Count > 0)
			{
				randomKey = unusedKeys.ElementAt(randon.Next(unusedKeys.Count));
				randomOrder.Add(randomKey);
				unusedKeys.Remove(randomKey);
			}

			foreach (Key key in randomOrder)
				rdnSequence.Append(KeyConverter.KeyToString(key, false));

			return rdnSequence.ToString();
		}

		void InteractionCtr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if ("HighlightedKey".Equals(e.PropertyName))
			{

			}
			else if ("SelectedKey".Equals(e.PropertyName))
			{
				AppendCharacterToEntryText();
			}
		}

		private void AppendCharacterToEntryText()
		{
			if (CoreInstance.InteractionCtr.SelectedKey == Key.OemPlus) //this is delete
			{
				if (tbEntry.Text.Length > 0)
				{
					tbFeedback.Text = "";
					tbFeedback.Background = Brushes.Transparent;
					tbEntry.Text = tbEntry.Text.Substring(0, tbEntry.Text.Length - 1);
				}
			}
			else if (CoreInstance.InteractionCtr.SelectedKey != Key.OemQuestion) //this is enter
				tbEntry.Text = tbEntry.Text + KeyConverter.KeyToString(CoreInstance.InteractionCtr.SelectedKey, true);
		}

		private void RemoveKeys(List<Key> keys)
		{
			keys.Remove(Key.B);
			keys.Remove(Key.E);
			keys.Remove(Key.G);
			keys.Remove(Key.I);
			keys.Remove(Key.J);
			keys.Remove(Key.L);
			keys.Remove(Key.N);
			keys.Remove(Key.Q);
			keys.Remove(Key.T);
			keys.Remove(Key.W);
			keys.Remove(Key.Y);
			keys.Remove(Key.D0);
			keys.Remove(Key.D1);
			keys.Remove(Key.D3);
			keys.Remove(Key.D5);
			keys.Remove(Key.D8);
		}
	}
}
