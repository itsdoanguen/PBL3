// Simple rich text editor for chapter content

document.addEventListener('DOMContentLoaded', function() {
	const toolbar = document.getElementById('chapterEditorToolbar');
	const contentArea = document.getElementById('chapterContentArea');
	const hiddenTextarea = document.getElementById('chapterContentHidden');

	function format(command) {
		document.execCommand(command, false, null);
		contentArea.focus();
	}

	if (toolbar && contentArea) {
		toolbar.querySelectorAll('button[data-cmd]').forEach(btn => {
			btn.addEventListener('click', function(e) {
				e.preventDefault();
				format(btn.dataset.cmd);
			});
		});
		// Sync content to hidden textarea before submit
		contentArea.addEventListener('input', function() {
			hiddenTextarea.value = contentArea.innerHTML;
		});
		// On form submit, ensure sync
		const form = contentArea.closest('form');
		if (form) {
			form.addEventListener('submit', function() {
				hiddenTextarea.value = contentArea.innerHTML;
			});
		}
	}
});
