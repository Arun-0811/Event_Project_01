/*Optional: Form Submission Handling*/
    
        document.getElementById('enquiryForm').addEventListener('submit', function(e) {
            e.preventDefault();
        alert('Thank you! Your enquiry has been submitted.');
        this.reset();
  });


