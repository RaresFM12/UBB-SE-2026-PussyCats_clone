using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Core;
using Windows.UI.Text;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class NoteViewModel : INotifyPropertyChanged
    {
        private readonly Action<NoteViewModel> deleteAction;
        private readonly Action<NoteViewModel> updateAction;
        private bool suppressPersistence;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (!suppressPersistence &&
                (propertyName == nameof(NoteBody) || propertyName == nameof(NoteIsDone)))
            {
                updateAction?.Invoke(this);
            }
        }

        public ICommand DeleteNoteCommand { get; }

        public int NoteId { get; }

        private string noteBody;
        public string NoteBody
        {
            get => noteBody;
            set
            {
                if (noteBody == value)
                {
                    return;
                }

                noteBody = value;
                OnPropertyChanged();
            }
        }

        private bool noteIsDone;
        public bool NoteIsDone
        {
            get => noteIsDone;
            set
            {
                if (noteIsDone == value)
                {
                    return;
                }

                noteIsDone = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoteBodyFontStyle));
            }
        }

        public FontStyle NoteBodyFontStyle => NoteIsDone ? FontStyle.Italic : FontStyle.Normal;

        public NoteViewModel(
            int noteId,
            string noteBody,
            bool noteIsDone,
            Action<NoteViewModel> deleteAction,
            Action<NoteViewModel> updateAction)
        {
            this.deleteAction = deleteAction;
            this.updateAction = updateAction;

            DeleteNoteCommand = new DelegateCommand(_ => this.deleteAction?.Invoke(this));

            NoteId = noteId;

            suppressPersistence = true;
            NoteBody = noteBody;
            NoteIsDone = noteIsDone;
            suppressPersistence = false;
        }
    }
}