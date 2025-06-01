// My Stories Page Interactivity
// Add hover effect, card animation, or future modal logic here

document.addEventListener('DOMContentLoaded', function() {
	// Example: Animate cards on scroll (if needed)
	const cards = document.querySelectorAll('.my-story-card');
	cards.forEach(card => {
		card.addEventListener('mouseenter', () => {
			card.classList.add('shadow-lg');
		});
		card.addEventListener('mouseleave', () => {
			card.classList.remove('shadow-lg');
		});
	});

	// Pagination logic
	const PAGE_SIZE = 12;
	const pagination = document.getElementById('myStoriesPagination');
	if (cards.length > PAGE_SIZE && pagination) {
		let currentPage = 1;
		const totalPages = Math.ceil(cards.length / PAGE_SIZE);

		function showPage(page) {
			cards.forEach((card, idx) => {
				if (idx >= (page - 1) * PAGE_SIZE && idx < page * PAGE_SIZE) {
					card.parentElement.style.display = '';
				} else {
					card.parentElement.style.display = 'none';
				}
			});
			// Update pagination
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
