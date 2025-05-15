/**
 * user-readchapter.js
 */

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.reply-btn').forEach(button => {
        button.addEventListener('click', function () {
            const commentId = this.getAttribute('data-comment-id');
            const userName = this.getAttribute('data-user-name');
            replyToComment(commentId, userName);
        });
    });

    initializeCommentScrolling();
});

/**
 * Handle phần reply tới comment
 * @param {string} commentId - id của phần comment đang reply
 * @param {string} userName - username của người dùng đang reply 
 */
window.replyToComment = function (commentId, userName) {
    const commentForm = document.getElementById("mainCommentForm").querySelector("form");

    // Update các field hidden trong form
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

    // Update type của comment
    const typeInput = commentForm.querySelector('input[name="Type"]');
    if (typeInput) {
        typeInput.value = "reply";
        console.log("Type input updated to:", typeInput.value);
    } else {
        // Nếu không có, tạo mới
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'Type';
        input.value = "reply";
        commentForm.appendChild(input);
        console.log("Type input created with value:", input.value);
    }

    // Show đang reply tới ai
    let replyingToDisplay = document.getElementById("replyingToDisplay");
    if (!replyingToDisplay) {
        replyingToDisplay = document.createElement('div');
        replyingToDisplay.id = 'replyingToDisplay';
        replyingToDisplay.className = 'alert alert-info mt-2';
        commentForm.querySelector('textarea').parentNode.insertBefore(replyingToDisplay, commentForm.querySelector('textarea'));
    }
    replyingToDisplay.innerHTML = `Đang trả lời <strong>${userName}</strong> <button type="button" id="cancelReplyBtn" class="btn btn-sm btn-outline-secondary float-end" onclick="cancelReply()">Hủy</button>`;
    replyingToDisplay.style.display = 'block';

    // Scroll tới comment form
    commentForm.scrollIntoView({ behavior: 'smooth' });

    setTimeout(() => {
        commentForm.querySelector('textarea').focus();
    }, 500);
};

/**
 * Hủy bỏ việc reply tới comment, trả về trạng thái ban đầu
 */
window.cancelReply = function () {
    const commentForm = document.getElementById("mainCommentForm").querySelector("form");

    // Reset parentCommentID
    const parentCommentInput = document.getElementById("parentCommentInput");
    if (parentCommentInput) {
        parentCommentInput.value = "";
    }

    // Reset form type
    const typeInput = commentForm.querySelector('input[name="Type"]');
    if (typeInput) {
        typeInput.value = "chapter";
        console.log("Type input reset to:", typeInput.value);
    }

    // Giấu phần đang reply
    const replyingToDisplay = document.getElementById("replyingToDisplay");
    if (replyingToDisplay) {
        replyingToDisplay.style.display = 'none';
    }
};

/**
 * Phần này để scroll đến comment cụ thể
 * Scroll đến comment dựa vào giá trị trong TempData
 */
function initializeCommentScrolling() {
    console.log("Checking for scrollToComment");

    // Nếu có giá trị commentId trong TempData, scroll đến comment đó
    const scrollToCommentInput = document.getElementById("scrollToComment");
    const scrollToComment = scrollToCommentInput?.value;
    console.log("scrollToComment value:", scrollToComment);

    if (scrollToComment) {
        // give timeout để đợi DOM render xong
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

                // C1: dùng scrollIntoView
                commentElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });

                // C2: scroll thủ công bằng window.scrollTo với offset
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

                // C3: dùng jQuery nếu có
                if (typeof $ !== 'undefined') {
                    setTimeout(() => {
                        $('html, body').animate({
                            scrollTop: $(commentElement).offset().top - 120
                        }, 1000);
                        console.log("jQuery scroll initiated");
                    }, 200);
                }

                commentElement.classList.add("comment-highlight");
                commentElement.style.animation = "pulse-highlight 2s";
                commentElement.style.boxShadow = "0 0 15px 5px rgba(255, 153, 0, 0.7)";

                setTimeout(() => {
                    commentElement.style.boxShadow = "none";
                    commentElement.style.animation = "";
                }, 3000);

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

// Mở/đóng sidebar
function toggleSidebar() {
    var sidebar = document.getElementById('chapterSidebar');
    var overlay = document.getElementById('overlay');
    if (sidebar.classList.contains('active')) {
        sidebar.classList.remove('active');
        overlay.style.display = 'none';
    } else {
        sidebar.classList.add('active');
        overlay.style.display = 'block';
    }
}

// Hiện thanh nav khi click vào chương
function showFloatingNav() {
    document.getElementById('floatingNav').style.display = 'flex';
}
document.addEventListener("DOMContentLoaded", function () {
    const commentsPerPage = 5;
    const commentList = document.getElementById("commentList");
    const comments = Array.from(commentList.children);
    const totalPages = Math.ceil(comments.length / commentsPerPage);
    const paginationControls = document.getElementById("paginationControls");

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

        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement("button");
            btn.innerText = i;
            btn.className = "btn btn-sm " + (i === currentPage ? "btn-primary" : "btn-outline-primary");
            btn.style.margin = "0 5px";
            btn.onclick = () => showPage(i);
            paginationControls.appendChild(btn);
        }
    }

    // Khởi tạo trang đầu tiên
    if (comments.length > commentsPerPage) {
        showPage(1);
    }
});