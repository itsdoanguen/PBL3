// Password strength & match check for Change Password page
function checkPasswordStrength(pw) {
	let score = 0;
	if (!pw) return 0;
	if (pw.length >= 6) score++;
	if (/[A-Z]/.test(pw)) score++;
	if (/[a-z]/.test(pw)) score++;
	if (/[0-9]/.test(pw)) score++;
	if (/[^A-Za-z0-9]/.test(pw)) score++;
	if (pw.length >= 10) score++;
	return score;
}

document.addEventListener('DOMContentLoaded', function() {
	const newPassword = document.getElementById('newPassword');
	const confirmPassword = document.getElementById('confirmPassword');
	const passwordStrength = document.getElementById('passwordStrength');
	const passwordMatch = document.getElementById('passwordMatch');

	function updateStrength() {
		const pw = newPassword.value;
		const score = checkPasswordStrength(pw);
		if (!pw) {
			passwordStrength.textContent = '';
			passwordStrength.className = '';
			return;
		}
		if (score <= 2) {
			passwordStrength.textContent = 'Yếu (ít nhất 6 ký tự, chữ hoa, số, ký tự đặc biệt)';
			passwordStrength.className = 'weak';
		} else if (score <= 4) {
			passwordStrength.textContent = 'Trung bình';
			passwordStrength.className = 'medium';
		} else {
			passwordStrength.textContent = 'Mạnh';
			passwordStrength.className = 'strong';
		}
	}

	function updateMatch() {
		if (!confirmPassword.value) {
			passwordMatch.textContent = '';
			passwordMatch.className = '';
			return;
		}
		if (newPassword.value === confirmPassword.value) {
			passwordMatch.textContent = 'Mật khẩu khớp';
			passwordMatch.className = 'match';
		} else {
			passwordMatch.textContent = 'Mật khẩu không khớp';
			passwordMatch.className = 'notmatch';
		}
	}

	newPassword.addEventListener('input', function() {
		updateStrength();
		updateMatch();
	});
	confirmPassword.addEventListener('input', updateMatch);
});
