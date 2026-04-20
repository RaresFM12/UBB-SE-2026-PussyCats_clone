using PharmacyApp.Features.Period_Tracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Assert.That(updateCalls, Is.EqualTo(1));
            Assert.That(updatedNote, Is.SameAs(viewModel));
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

            Assert.That(updateCalls, Is.EqualTo(1));
            Assert.That(viewModel.NoteBodyFontStyle, Is.EqualTo(FontStyle.Italic));
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
    }
}
