// Follow Stories page: pagination logic

document.addEventListener('DOMContentLoaded', function() {
	const PAGE_SIZE = 6;
	const storyCards = document.querySelectorAll('.follow-story-card');
	const pagination = document.getElementById('followPagination');
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
});
