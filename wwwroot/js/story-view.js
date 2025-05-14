/**
 * story-view.js
 * Handles comments and interaction functionality for story detail page
 */

document.addEventListener("DOMContentLoaded", function () {
    // Attach event listeners to reply buttons
    document.querySelectorAll('.reply-btn').forEach(button => {
        button.addEventListener('click', function () {
            const commentId = this.getAttribute('data-comment-id');
            const userName = this.getAttribute('data-user-name');
            replyToComment(commentId, userName);
        });
    });

    // Initialize comment scrolling if needed
    initializeCommentScrolling();

    // Initialize comment pagination
    initializeCommentPagination();
});

/**
 * Handle replying to a comment
 * @param {string} commentId - ID of the comment being replied to
 * @param {string} userName - Username of the person who wrote the comment
 */
window.replyToComment = function (commentId, userName) {
    const commentForm = document.getElementById("mainCommentForm").querySelector("form");

    // Update hidden fields in the form
    const parentCommentInput = commentForm.querySelector('input[name="ParentCommentID"]');
    if (!parentCommentInput) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'ParentCommentID';
        input.id = 'parentCommentInput';
        input.value = commentId;
        commentForm.appendChild(input);
    } else {
        parentCommentInput.value = commentId;
    }

    // Update comment type
    const typeInput = commentForm.querySelector('input[name="Type"]');
    if (typeInput) {
        typeInput.value = "reply";
        console.log("Type input updated to:", typeInput.value);
    } else {
        // Create if it doesn't exist
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'Type';
        input.value = "reply";
        commentForm.appendChild(input);
        console.log("Type input created with value:", input.value);
    }

    // Show who you're replying to
    let replyingToDisplay = document.getElementById("replyingToDisplay");
    if (!replyingToDisplay) {
        replyingToDisplay = document.createElement('div');
        replyingToDisplay.id = 'replyingToDisplay';
        replyingToDisplay.className = 'alert alert-info mt-2';
        commentForm.querySelector('textarea').parentNode.insertBefore(replyingToDisplay, commentForm.querySelector('textarea'));
    }
    replyingToDisplay.innerHTML = `Đang trả lời <strong>${userName}</strong> <button type="button" id="cancelReplyBtn" class="btn btn-sm btn-outline-secondary float-end" onclick="cancelReply()">Hủy</button>`;
    replyingToDisplay.style.display = 'block';

    // Scroll to comment form
    commentForm.scrollIntoView({ behavior: 'smooth' });

    // Focus the textarea
    setTimeout(() => {
        commentForm.querySelector('textarea').focus();
    }, 500);
};

/**
 * Cancel replying to a comment, reset form to initial state
 */
window.cancelReply = function () {
    const commentForm = document.getElementById("mainCommentForm").querySelector("form");

    // Reset parentCommentID
    const parentCommentInput = document.getElementById("parentCommentInput");
    if (parentCommentInput) {
        parentCommentInput.value = "";
    }

    // Reset form type back to story
    const typeInput = commentForm.querySelector('input[name="Type"]');
    if (typeInput) {
        typeInput.value = "story";
        console.log("Type input reset to:", typeInput.value);
    }

    // Hide replying display
    const replyingToDisplay = document.getElementById("replyingToDisplay");
    if (replyingToDisplay) {
        replyingToDisplay.style.display = 'none';
    }
};

/**
 * Initialize comment scrolling functionality
 * Scrolls to specific comment based on TempData value
 */
function initializeCommentScrolling() {
    console.log("Checking for scrollToComment");

    // Check if there's a comment ID in TempData
    const scrollToCommentInput = document.getElementById("scrollToComment");
    const scrollToComment = scrollToCommentInput?.value;
    console.log("scrollToComment value:", scrollToComment);

    if (scrollToComment) {
        // Give timeout to wait for DOM to render
        setTimeout(() => {
            let commentElement = document.getElementById(scrollToComment);
            if (!commentElement) {
                commentElement = document.getElementById(`comment-${scrollToComment}`);
            }
            if (!commentElement && scrollToComment.startsWith("comment-")) {
                const commentId = scrollToComment.replace("comment-", "");
                commentElement = document.getElementById(commentId);
            }

            console.log("Found comment element:", commentElement);

            if (commentElement) {
                console.log("Attempting to scroll to element");

                // Method 1: Use scrollIntoView
                commentElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });

                // Method 2: Manual scroll with offset
                setTimeout(() => {
                    const headerOffset = 120;
                    const elementPosition = commentElement.getBoundingClientRect().top;
                    const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                    window.scrollTo({
                        top: offsetPosition,
                        behavior: "smooth"
                    });
                    console.log("Manual scroll to position:", offsetPosition);
                }, 100);

                // Method 3: Use jQuery if available
                if (typeof $ !== 'undefined') {
                    setTimeout(() => {
                        $('html, body').animate({
                            scrollTop: $(commentElement).offset().top - 120
                        }, 1000);
                        console.log("jQuery scroll initiated");
                    }, 200);
                }

                // Highlight the comment
                commentElement.classList.add("comment-highlight");
                commentElement.style.animation = "pulse-highlight 2s";
                commentElement.style.boxShadow = "0 0 15px 5px rgba(255, 153, 0, 0.7)";

                // Remove highlight after 3 seconds
                setTimeout(() => {
                    commentElement.style.boxShadow = "none";
                    commentElement.style.animation = "";
                }, 3000);

                // Update URL hash
                history.pushState(null, null, `#${scrollToComment}`);
            } else {
                console.error("Comment element not found. Debug info:");
                console.log("All comment IDs on page:");
                document.querySelectorAll('[id^="comment-"]').forEach(el => {
                    console.log(el.id);
                });
            }
        }, 1000);
    } else {
        console.log("No scrollToComment value found");
    }
}

/**
 * Initialize pagination for comments
 * Shows a limited number of comments per page
 */
function initializeCommentPagination() {
    const commentsPerPage = 5;
    const commentList = document.getElementById("commentList");

    if (!commentList) return;

    const comments = Array.from(commentList.children);
    const totalPages = Math.ceil(comments.length / commentsPerPage);
    const paginationControls = document.getElementById("paginationControls");

    if (!paginationControls || comments.length <= commentsPerPage) return;

    function showPage(page) {
        const start = (page - 1) * commentsPerPage;
        const end = start + commentsPerPage;

        comments.forEach((comment, index) => {
            comment.style.display = (index >= start && index < end) ? "block" : "none";
        });

        renderPagination(page);
    }

    function renderPagination(currentPage) {
        paginationControls.innerHTML = "";

        // Add previous button if not on first page
        if (currentPage > 1) {
            const prevBtn = document.createElement("button");
            prevBtn.innerHTML = "&laquo;";
            prevBtn.className = "btn btn-sm btn-outline-primary";
            prevBtn.style.margin = "0 5px";
            prevBtn.onclick = () => showPage(currentPage - 1);
            paginationControls.appendChild(prevBtn);
        }

        // Add page buttons
        for (let i = 1; i <= totalPages; i++) {
            // Only show a few pages around current page if there are many
            if (totalPages > 7 && (i < currentPage - 2 || i > currentPage + 2) && i !== 1 && i !== totalPages) {
                if (i === currentPage - 3 || i === currentPage + 3) {
                    const ellipsis = document.createElement("span");
                    ellipsis.innerText = "...";
                    ellipsis.style.margin = "0 5px";
                    paginationControls.appendChild(ellipsis);
                }
                continue;
            }

            const btn = document.createElement("button");
            btn.innerText = i;
            btn.className = "btn btn-sm " + (i === currentPage ? "btn-primary" : "btn-outline-primary");
            btn.style.margin = "0 5px";
            btn.onclick = () => showPage(i);
            paginationControls.appendChild(btn);
        }

        // Add next button if not on last page
        if (currentPage < totalPages) {
            const nextBtn = document.createElement("button");
            nextBtn.innerHTML = "&raquo;";
            nextBtn.className = "btn btn-sm btn-outline-primary";
            nextBtn.style.margin = "0 5px";
            nextBtn.onclick = () => showPage(currentPage + 1);
            paginationControls.appendChild(nextBtn);
        }
    }

    // Initialize first page
    showPage(1);
}

/**
 * Handle smooth scrolling to elements when clicking anchor links
 */
document.addEventListener('click', function (event) {
    // Check if clicked element is an anchor link with hash
    if (event.target.tagName === 'A' && event.target.hash) {
        const targetElement = document.querySelector(event.target.hash);
        if (targetElement) {
            event.preventDefault();
            targetElement.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });

            // Update URL without causing jump
            history.pushState(null, null, event.target.hash);
        }
    }
});