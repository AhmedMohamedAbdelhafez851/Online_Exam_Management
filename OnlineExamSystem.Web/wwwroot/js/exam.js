$(document).ready(function () {
    $('#examForm').submit(function (e) {
        e.preventDefault();
        let answers = {};
        $('input[type=radio]:checked').each(function () {
            answers[$(this).attr('name')] = $(this).val();
        });
        $.ajax({
            url: '/Exam/SubmitExam',
            method: 'POST',
            data: { examId: $('input[name=examId]').val(), answers: answers },
            success: function (response) {
                $('#resultContainer').html(response);
            },
            error: function (xhr, status, error) {
                $('#resultContainer').html('<p class="text-danger">Error submitting exam: ' + error + '</p>');
            }
        });
    });
});