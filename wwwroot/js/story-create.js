// Story Create Genre Tag UI

document.addEventListener('DOMContentLoaded', function() {
	const genreTagList = document.getElementById('genreTagList');
	const addGenreBtn = document.getElementById('btnAddGenre');
	const genreDropdown = document.getElementById('genreDropdown');
	const genreDropdownItems = genreDropdown ? genreDropdown.querySelectorAll('.genre-dropdown-item') : [];
	const genreInput = document.getElementById('GenreIDsInput');

	function updateGenreInput() {
		const selected = Array.from(genreTagList.querySelectorAll('.genre-tag')).map(tag => tag.dataset.genreId);
		genreInput.value = selected.join(',');
	}

	if (addGenreBtn && genreDropdown) {
		addGenreBtn.addEventListener('click', function(e) {
			e.stopPropagation();
			genreDropdown.classList.toggle('show');
		});
		// Hide dropdown when clicking outside
		document.addEventListener('click', function(e) {
			if (!genreDropdown.contains(e.target) && e.target !== addGenreBtn) {
				genreDropdown.classList.remove('show');
			}
		});
		// Add genre from dropdown
		genreDropdownItems.forEach(item => {
			item.addEventListener('click', function() {
				const genreId = this.dataset.genreId;
				const genreName = this.textContent;
				if (!genreTagList.querySelector(`[data-genre-id='${genreId}']`)) {
					const tag = document.createElement('span');
					tag.className = 'genre-tag';
					tag.dataset.genreId = genreId;
					tag.innerHTML = `${genreName}<button type='button' class='remove-genre' title='Xóa'>&times;</button>`;
					genreTagList.insertBefore(tag, addGenreBtn.parentElement);
					updateGenreInput();
					tag.querySelector('.remove-genre').onclick = function() {
						tag.remove();
						updateGenreInput();
					};
				}
				genreDropdown.classList.remove('show');
			});
		});
	}
	// Remove genre tag
	genreTagList.querySelectorAll('.remove-genre').forEach(btn => {
		btn.onclick = function() {
			btn.parentElement.remove();
			updateGenreInput();
		};
	});
	updateGenreInput();
});
