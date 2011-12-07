using System;
using System.Xml;
using System.Collections.Generic;

namespace Agouti
{
    /// <summary>
    /// Agouti interface to a testing browser (HtmlUnit or Selenium)
    /// </summary>
    public interface IBrowser : IDisposable
    {
        void Visit(string url);
        void VisitAjax(string url);

        void ShouldHaveTitle(string testPageContent);


        #region ------ Filling in form data ------

        void FillInTextBox(string inputFieldname, string inputValue);
        void FillInTextBox(string inputFieldname, int elementIndex, string inputValue);
        void FillInTextBox(XPathSelector selector, string inputValue);

        void FillInTextArea(string inputFieldname, string inputValue);
        void FillInTextArea(XPathSelector selector, string inputValue); 
        
        void SelectFromDropdown(string dropDownName, string optionText);

        void SelectRadioButton(string radioGroupName, string selectedValue);

        void SetCheckbox(string name, bool isChecked);
        void SetCheckboxById(string id, bool isChecked);
        void SetCheckboxByValue(string value, bool isChecked);
        void SetCheckbox(XPathSelector selector, bool isChecked);

        void SubmitForm();
        void SubmitForm(XPathSelector selector);

        #endregion


        void ShouldContain(string expectedText);

        void ShouldNotContain(string expectedText);

        void ShouldContain(XPathSelector selector);

        void ShouldContainExactly(int expectedCount, XPathSelector selector);

        void ShouldBeAtUrl(string expectedUrl);

        void ShouldBeAtUrlIgnoringQuerystring(string expectedUrl);

        void Click(XPathSelector clickableXPath);

        string CurrentPageUrl { get;}
        int CurrentPageHttpCode { get; }
        string CurrentPageAsText { get; }

        IEnumerable<XmlNode> Select(XPathSelector selector);
        string Download(XPathSelector selector);
        void ShouldEventually(Func<IBrowser, int, bool> task, int timesToRetry, int millisecondDelay);

        int WaitForBackgroundJavaScript(int millisecondDelay);        
    }
}