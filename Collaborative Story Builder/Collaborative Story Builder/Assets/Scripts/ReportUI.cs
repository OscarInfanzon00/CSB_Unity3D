using UnityEngine;
using UnityEngine.UI;

public class ReportUI : MonoBehaviour
{
    public ReportSystem reportManager;
    public Dropdown reasonDropdown;
    public InputField detailsInput;
    public string reportedUserId;
    public string reporterUserId;

    public void SubmitReport()
    {
        string reason = reasonDropdown.options[reasonDropdown.value].text;
        string additionalDetails = detailsInput.text;

        reportManager.ReportUser(reportedUserId, reporterUserId, reason, additionalDetails);
    }
}
