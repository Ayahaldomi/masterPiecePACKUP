$(document).ready(function() {
    var currentStep = 1;
    var totalSteps = $(".form-step").length;

    function updateStepBar() {
      $(".step").removeClass("active");
      $("#step-" + currentStep + "-bar").addClass("active");
    }

    $(".next-btn").click(function() {
      if (currentStep < totalSteps) {
        $("#step-" + currentStep).removeClass("active");
        currentStep++;
        $("#step-" + currentStep).addClass("active");
        updateStepBar();
      }
    });

    $(".prev-btn").click(function() {
      if (currentStep > 1) {
        $("#step-" + currentStep).removeClass("active");
        currentStep--;
        $("#step-" + currentStep).addClass("active");
        updateStepBar();
      }
    });
  });
