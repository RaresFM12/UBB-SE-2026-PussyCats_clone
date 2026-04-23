using PharmacyApp.Features.Period_Tracker.ViewModels;
using Windows.UI.Text;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.ViewModels
{
    [TestFixture]
    public class NoteViewModelTests
    {
        [Test]
        public void Constructor_WhenCreated_DoesNotTriggerUpdateAction()
        {
            int updateCalls = 0;

            _ = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: _ => updateCalls++);

            Assert.That(updateCalls, Is.EqualTo(0));
        }

        [Test]
        public void NoteBody_WhenChanged_TriggersUpdateAction()
        {
            int updateCalls = 0;
            NoteViewModel? updatedNote = null;

            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: note =>
                {
                    updateCalls++;
                    updatedNote = note;
                });

            viewModel.NoteBody = "Changed";

            Assert.That(
                UpdateActionMatches(updateCalls, updatedNote, viewModel),
                Is.True);
        }

        [Test]
        public void NoteIsDone_WhenChanged_TriggersUpdateActionAndChangesFontStyle()
        {
            int updateCalls = 0;

            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: _ => updateCalls++);

            viewModel.NoteIsDone = true;

            Assert.That(
                IsDoneUpdateMatches(updateCalls, viewModel.NoteBodyFontStyle),
                Is.True);
        }

        [Test]
        public void DeleteNoteCommand_WhenExecuted_InvokesDeleteActionWithCurrentInstance()
        {
            NoteViewModel? deletedNote = null;

            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: note => deletedNote = note,
                updateNoteAction: _ => { });

            viewModel.DeleteNoteCommand.Execute(null);

            Assert.That(deletedNote, Is.SameAs(viewModel));
        }

        [Test]
        public void Constructor_WhenCreatedWithIncompleteNote_HasNormalFontStyle()
        {
            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: _ => { });

            Assert.That(viewModel.NoteBodyFontStyle, Is.EqualTo(FontStyle.Normal));
        }

        [Test]
        public void NoteBody_WhenSetToSameValue_DoesNotTriggerUpdateAction()
        {
            int updateCalls = 0;

            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: _ => updateCalls++);

            viewModel.NoteBody = "Initial";

            Assert.That(updateCalls, Is.EqualTo(0));
        }

        [Test]
        public void NoteIsDone_WhenSetToSameValue_DoesNotTriggerUpdateAction()
        {
            int updateCalls = 0;

            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: _ => updateCalls++);

            viewModel.NoteIsDone = false;

            Assert.That(updateCalls, Is.EqualTo(0));
        }

        [Test]
        public void DeleteNoteCommand_WhenDeleteActionIsNull_DoesNotThrow()
        {
            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: null!,
                updateNoteAction: _ => { });

            Assert.DoesNotThrow(() => viewModel.DeleteNoteCommand.Execute(null));
        }

        [Test]
        public void OnPropertyChanged_WhenUpdateActionIsNull_DoesNotThrow()
        {
            NoteViewModel viewModel = new NoteViewModel(
                1,
                "Initial",
                false,
                deleteNoteAction: _ => { },
                updateNoteAction: null!);

            Assert.DoesNotThrow(() => viewModel.NoteBody = "Changed");
        }

        private static bool UpdateActionMatches(
            int updateCalls,
            NoteViewModel? updatedNote,
            NoteViewModel expectedNote)
        {
            return updateCalls == 1 && ReferenceEquals(updatedNote, expectedNote);
        }

        private static bool IsDoneUpdateMatches(int updateCalls, FontStyle fontStyle)
        {
            return updateCalls == 1 && fontStyle == FontStyle.Italic;
        }
    }
}