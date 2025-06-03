// Dashboard Stories Slider JavaScript
// Optimized and Centralized Slider Logic

class DashboardSlider {
    constructor() {
        this.sliderStates = {};
        this.init();
    }

    init() {
        document.addEventListener('DOMContentLoaded', () => {
            this.initializeAllSliders();
            this.setupCategoryFilter();
            this.setupWindowResize();
            this.initializeAnimations();
        });
    }

    initializeSlider(containerName) {
        const sliderElement = document.querySelector(`[data-container="${containerName}"] .stories-slider`);
        if (!sliderElement) return;

        const containerWidth = sliderElement.offsetWidth;
        const itemWidth = 236; // 220px + 16px gap
        const visibleItems = Math.floor(containerWidth / itemWidth);

        this.sliderStates[containerName] = {
            currentPosition: 0,
            itemWidth: itemWidth,
            visibleItems: Math.max(1, visibleItems),
            totalItems: 0
        };

        // Calculate total items
        const track = sliderElement.querySelector('.stories-track');
        if (track) {
            this.sliderStates[containerName].totalItems = track.querySelectorAll('.story-item').length;
        }
    }

    slideStories(containerName, direction) {
        // Initialize if not exists
        if (!this.sliderStates[containerName]) {
            this.initializeSlider(containerName);
        }

        const container = document.querySelector(`[data-container="${containerName}"]`);
        if (!container) return;

        const track = container.querySelector('.stories-track');
        const items = track.querySelectorAll('.story-item');
        const state = this.sliderStates[containerName];

        if (!state || items.length === 0) return;

        // Update total items if changed
        state.totalItems = items.length;
        const maxPosition = Math.max(0, state.totalItems - state.visibleItems);

        // Calculate new position
        state.currentPosition += direction;
        state.currentPosition = Math.max(0, Math.min(state.currentPosition, maxPosition));

        // Apply transform
        const translateX = -state.currentPosition * state.itemWidth;
        track.style.transform = `translateX(${translateX}px)`;

        // Update arrow states
        this.updateArrowStates(container, state, maxPosition);
    }

    updateArrowStates(container, state, maxPosition) {
        const prevBtn = container.querySelector('.nav-arrow.prev');
        const nextBtn = container.querySelector('.nav-arrow.next');

        if (prevBtn) {
            prevBtn.disabled = state.currentPosition === 0;
            prevBtn.classList.toggle('disabled', state.currentPosition === 0);
        }

        if (nextBtn) {
            nextBtn.disabled = state.currentPosition >= maxPosition;
            nextBtn.classList.toggle('disabled', state.currentPosition >= maxPosition);
        }
    }

    initializeAllSliders() {
        const containers = document.querySelectorAll('[data-container]');
        containers.forEach(container => {
            const containerName = container.dataset.container;
            this.initializeSlider(containerName);
            this.slideStories(containerName, 0); // Initialize with no movement
        });
    }

    setupCategoryFilter() {
        const categoryFilter = document.getElementById('categoryFilter');
        if (!categoryFilter) return;

        categoryFilter.addEventListener('change', (e) => {
            const categoryId = e.target.value;
            const hotStoriesContainer = document.getElementById('hotStoriesContainer');

            if (!hotStoriesContainer) return;

            // Show loading state
            hotStoriesContainer.classList.add('loading');

            // AJAX call
            fetch('/User/GetHotStoriesByCategory?categoryId=' + categoryId, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
            .then(response => response.text())
            .then(data => {
                hotStoriesContainer.innerHTML = data;
                hotStoriesContainer.classList.remove('loading');
                
                // Reinitialize slider after content update
                delete this.sliderStates['hot'];
                this.initializeSlider('hot');
                this.slideStories('hot', 0);
            })
            .catch(error => {
                console.error('Error loading stories:', error);
                hotStoriesContainer.classList.remove('loading');
                this.showToast('Đã xảy ra lỗi khi tải dữ liệu.', 'error');
            });
        });
    }

    setupWindowResize() {
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                Object.keys(this.sliderStates).forEach(containerName => {
                    delete this.sliderStates[containerName];
                    this.initializeSlider(containerName);
                    this.slideStories(containerName, 0);
                });
            }, 250);
        });
    }

    setupUnfollowStory() {
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('unfollow-story')) {
                e.preventDefault();
                const storyId = e.target.dataset.storyId;
                const button = e.target;

                if (confirm('Bạn có chắc chắn muốn bỏ theo dõi truyện này?')) {
                    fetch('/Follow/UnfollowStory', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({ storyId: storyId })
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            button.closest('.story-item').style.display = 'none';
                            this.showToast('Đã bỏ theo dõi truyện', 'success');
                        } else {
                            this.showToast(data.message || 'Có lỗi xảy ra', 'error');
                        }
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        this.showToast('Có lỗi xảy ra khi bỏ theo dõi', 'error');
                    });
                }
            }
        });
    }

    initializeAnimations() {
        // Initialize AOS if available
        if (typeof AOS !== 'undefined') {
            AOS.init({
                duration: 600,
                easing: 'ease-out',
                once: true,
                offset: 100
            });
        }
    }

    showToast(message, type = 'info') {
        // Use toastr if available, otherwise fallback to alert
        if (typeof toastr !== 'undefined') {
            toastr[type](message);
        } else {
            alert(message);
        }
    }

    // Touch support for mobile
    setupTouchSupport() {
        let startX = 0;
        let startY = 0;
        let isSwipe = false;

        document.addEventListener('touchstart', (e) => {
            if (e.target.closest('.stories-slider')) {
                startX = e.touches[0].clientX;
                startY = e.touches[0].clientY;
                isSwipe = true;
            }
        });

        document.addEventListener('touchmove', (e) => {
            if (!isSwipe) return;

            const deltaY = Math.abs(e.touches[0].clientY - startY);
            if (deltaY > 50) {
                isSwipe = false; // Prevent horizontal swipe if vertical scroll
            }
        });

        document.addEventListener('touchend', (e) => {
            if (!isSwipe) return;

            const endX = e.changedTouches[0].clientX;
            const deltaX = startX - endX;

            if (Math.abs(deltaX) > 50) { // Minimum swipe distance
                const slider = e.target.closest('.stories-slider');
                if (slider) {
                    const container = slider.closest('[data-container]');
                    if (container) {
                        const containerName = container.dataset.container;
                        const direction = deltaX > 0 ? 1 : -1;
                        this.slideStories(containerName, direction);
                    }
                }
            }

            isSwipe = false;
        });
    }
}

// Global functions for backwards compatibility
let dashboardSlider;

function slideStories(containerName, direction) {
    if (dashboardSlider) {
        dashboardSlider.slideStories(containerName, direction);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    dashboardSlider = new DashboardSlider();
    dashboardSlider.setupTouchSupport();
    dashboardSlider.setupUnfollowStory();
});

// Export for module usage if needed
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DashboardSlider;
} 