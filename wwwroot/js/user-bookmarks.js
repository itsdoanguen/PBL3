// Bookmarks page: pagination & collapse logic

document.addEventListener('DOMContentLoaded', function() {
	const PAGE_SIZE = 5;
	const storyCards = document.querySelectorAll('.bookmark-story-card');
	const pagination = document.getElementById('bookmarkPagination');
	if (storyCards.length > PAGE_SIZE && pagination) {
		let currentPage = 1;
		const totalPages = Math.ceil(storyCards.length / PAGE_SIZE);
		function showPage(page) {
			storyCards.forEach((card, idx) => {
				if (idx >= (page - 1) * PAGE_SIZE && idx < page * PAGE_SIZE) {
					card.style.display = '';
				} else {
					card.style.display = 'none';
				}
			});
			pagination.innerHTML = '';
			for (let i = 1; i <= totalPages; i++) {
				const li = document.createElement('li');
				li.className = 'page-item' + (i === page ? ' active' : '');
				const btn = document.createElement('button');
				btn.className = 'page-link';
				btn.textContent = i;
				btn.onclick = () => {
					currentPage = i;
					showPage(currentPage);
					window.scrollTo({top: pagination.offsetTop - 120, behavior: 'smooth'});
				};
				li.appendChild(btn);
				pagination.appendChild(li);
			}
		}
		showPage(currentPage);
	}
	// Collapse logic: close others when open one
	const collapseBtns = document.querySelectorAll('.btn-toggle-chapters');
	collapseBtns.forEach(btn => {
		btn.addEventListener('click', function() {
			const targetId = btn.getAttribute('data-bs-target');
			const allCollapse = document.querySelectorAll('.bookmark-chapter-list.collapse');
			allCollapse.forEach(col => {
				if ('#' + col.id !== targetId) col.classList.remove('show');
			});
		});
	});
});
