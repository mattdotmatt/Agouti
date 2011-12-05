using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Pyramid2.GuiTests.Agouti;
using com.gargoylesoftware.htmlunit;
using com.gargoylesoftware.htmlunit.html;
using NUnit.Framework;

namespace Agouti
{
    
    public class HtmlUnitAdapter : IBrowser
    {
        private readonly ITime _time;
        public const string FORM_INPUT_TYPE_SUBMIT = "//form[1]//input[@type='submit'] | //form[1]//button[@type='submit']";
        private readonly WebClient _browser;

        private HtmlPage _lastPage;


        public HtmlUnitAdapter(ITime time)
        {
            _time = time;
            _browser = new WebClient();
            _browser.setJavaScriptEnabled(false);
        }

        private T WrapError<T>(Func<T> getDelegate)
        {
            try
            {
                return getDelegate();
            }
            catch (FailingHttpStatusCodeException ex)
            {
                if (ex.getStatusCode() == 500)
                {
                    var response = ex.getResponse();
                    var parse = HTMLParser.parseHtml(response, _browser.getCurrentWindow());
                    var error = parse.getFirstByXPath("//h2/i/text()").ToString();

                    throw new Http500Exception(error);
                }

                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("{0} occured on page {1}\n{2}\n{3}",ex.GetType().Name, CurrentPageUrl, ex.Message, _lastPage.asText()));
            }
        }

        public void Visit(string url)
        {
            _lastPage = WrapError(() => _browser.getPage(url) as HtmlPage);
        }

        public string PageTitle { get { return _lastPage.getTitleText(); } }

        public void ShouldHaveTitle(string expectedTitle)
        {
            Assert.That(PageTitle, Is.EqualTo(expectedTitle),
                "Expected the page title to be '{0}', but it was '{1}'",
                expectedTitle, PageTitle);
        }

        public void FillInTextBox(string inputFieldname, string inputValue)
        {
            var element = WrapError(() => _lastPage.getElementByName(inputFieldname) as HtmlInput);
            element.setValueAttribute(inputValue);
        }

        public void FillInTextBox(string inputFieldname, int elementIndex, string inputValue)
        {
            var element = WrapError(() => (_lastPage.getElementsByName(inputFieldname).get(elementIndex)) as HtmlInput);
            element.setValueAttribute(inputValue);
        }

        public void FillInTextArea(string inputFieldname, string inputValue)
        {
            var element = (HtmlTextArea)_lastPage.getElementByName(inputFieldname);
            element.setText(inputValue);
        }

        public void FillInTextBox(XPathSelector selector, string inputValue)
        {
            var element = WrapError(() => GetSingleElement<HtmlInput>(selector));
            element.setValueAttribute(inputValue);
        }

        public void FillInTextArea(XPathSelector selector, string inputValue)
        {
            var element = WrapError(() => GetSingleElement<HtmlTextArea>(selector));
            element.setText(inputValue);
        }

        public void FillInForm(XPathSelector selector, string inputValue)
        {
            var node = GetSingleElement<HtmlInput>(selector);
            node.setValueAttribute(inputValue);
        }

        private T GetSingleElement<T>(XPathSelector selector)
        {
            var nodes = WrapError(() => (_lastPage.getByXPath(selector.XPath).toArray().OfType<T>().ToArray()));
            Assert.That(nodes.Length, Is.EqualTo(1), String.Format("Expected one element with the XPath {0} on page {1}",selector.XPath, CurrentPageAsText));
            return nodes[0];
        }

        public void SelectFromDropdown(string dropDownName, string optionText)
        {
            var element = (HtmlSelect)_lastPage.getElementByName(dropDownName);
            var option = (HtmlOption)element.getByXPath(String.Format("option[text()='{0}']",optionText)).toArray().Single();
            //var option = array.OfType<HtmlOption>().Single(o => o.getNodeValue().Equals(optionText, StringComparison.CurrentCultureIgnoreCase));
            option.setSelected(true);
        }

        public void SelectRadioButton(string radioGroupName, string selectedValue)
        {
            var option = (HtmlRadioButtonInput)_lastPage.getByXPath(String.Format("//input[@type='radio' and @name='{0}' and @value='{1}']",radioGroupName, selectedValue)).toArray().Single();
            option.setChecked(true);
        }

        public void SetCheckboxByValue(string value, bool isChecked)
        {
            var element = (HtmlCheckBoxInput)_lastPage.getByXPath(String.Format("//input[@type='checkbox' and @value='{0}']",value)).toArray().Single();
            element.setChecked(isChecked);
        }

        public void SetCheckbox(XPathSelector selector, bool isChecked)
        {
            var element = (HtmlCheckBoxInput)_lastPage.getByXPath(selector.XPath).toArray().Single();
            element.setChecked(isChecked);
        }

        public void SetCheckbox(string name, bool isChecked)
        {
            var element = (HtmlCheckBoxInput)_lastPage.getByXPath(String.Format("//input[@type='checkbox'  and following-sibling::text()='{0}']",name)).toArray().Single();
            element.setChecked(isChecked);
        }

        public void SetCheckboxById(string id, bool isChecked)
        {
            var element = (HtmlCheckBoxInput)_lastPage.getByXPath(String.Format("//input[@type='checkbox' and @id='{0}']",id)).toArray().Single();
            element.setChecked(isChecked);
        }

        public void SubmitForm()
        {
            SubmitForm( new XPathSelector(FORM_INPUT_TYPE_SUBMIT));
        }

        public void SubmitForm(XPathSelector selector)
        {
            Click(selector);
        }

        public void ShouldContain(string expectedText)
        {
            Assert.That(_lastPage.asText(), Is.StringContaining(expectedText), "Expected the page to contain '{0}'", expectedText);
        }

        public void ShouldNotContain(string expectedText)
        {
            Assert.That(_lastPage.asText(), Is.Not.StringContaining(expectedText), "Expected the page to not contain '{0}'", expectedText);
        }


        public void ShouldContain(XPathSelector selector)
        {
            var results = GetElementsFromPageByXPath(selector);
            Assert.That(results.Count, Is.GreaterThanOrEqualTo(1), "Unable to find any nodes matching '{0}' for page:\n {1}",
                selector, _lastPage.asText());
        }

        public void ShouldContainExactly(int expectedCount, XPathSelector selector)
        {
            var results = GetElementsFromPageByXPath(selector);
            Assert.That(results.Count, Is.EqualTo(expectedCount), "Found {0} nodes matching '{1}, expected {2}' for page:\n {3}",
                        results.Count, selector, expectedCount, _lastPage.asText());
        }

        public void ShouldBeAtUrl(string expectedUrl)
        {
            ShouldBeAtUrl(expectedUrl, false);
        }

        public void ShouldBeAtUrlIgnoringQuerystring(string expectedUrl)
        {
            ShouldBeAtUrl(expectedUrl, true);
        }

        private void ShouldBeAtUrl(string expectedUrl, bool removeQueryString)
        {
            var pageUrl = CurrentPageUrl;

            pageUrl = !removeQueryString ? pageUrl : pageUrl.Split('?')[0].TrimEnd('/');

            Assert.That(pageUrl.EndsWith(expectedUrl, StringComparison.CurrentCultureIgnoreCase),
                        "Expected the current URL '{0}' to be but was '{1}'", expectedUrl, pageUrl);
        }


        public void Click(XPathSelector selector)
        {
            var elementsFromPageByXPath = GetSingleElementFromPageByXPath(selector);
            
            _lastPage = WrapError(() => elementsFromPageByXPath.click() as HtmlPage);
        }

        public string Download(XPathSelector selector)
        {
            var elementsFromPageByXPath = GetSingleElementFromPageByXPath(selector);

            var page = WrapError(() => elementsFromPageByXPath.click() as TextPage);

            return page.getContent();
        }

        public void ShouldEventually(Func<IBrowser,int,bool> task, int timesToRetry, int millisecondDelay)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Predicate failed to pass after {0} retries", timesToRetry);

            for (var ctr = 0; ctr <= timesToRetry; ctr++)
            {
                if (task(this, ctr)) return;
                sb.AppendLine(String.Format("Try {0}: Failed to execute predicate at {1}",ctr, DateTime.Now));

                _time.Sleep(millisecondDelay);
            }

            Assert.Fail(sb.ToString());
        }

        public string CurrentPageUrl
        {
            get { return _lastPage.getWebResponse().getWebRequest().getUrl().toString(); }
        }

        public int CurrentPageHttpCode
        {
            get { return _lastPage.getWebResponse().getStatusCode(); }
        }

        public string CurrentPageAsText { get { return _lastPage.asText(); } }

        public IEnumerable<XmlNode> Select(XPathSelector selector)
        {
            var nodes = _lastPage.getByXPath(selector.XPath).toArray();
            foreach (DomNode node in nodes)
            {
                var doc = new XmlDocument();

                // is this a text node or not
                if (node is HtmlElement)
                {
                    doc.LoadXml(node.asXml());
                    yield return doc.DocumentElement;
                }
                else
                {
                    yield return doc.CreateTextNode(node.asText());
                }

            }
        }



        #region ------ Private helper methods ------

        private List<HtmlElement> GetElementsFromPageByXPath(XPathSelector selector)
        {
            return _lastPage.getByXPath(selector.XPath).toArray().Cast<HtmlElement>().ToList();
        }

        private HtmlElement GetSingleElementFromPageByXPath(XPathSelector selector)
        {
            var elementsFromPageByXPath = GetElementsFromPageByXPath(selector);

            Assert.That(elementsFromPageByXPath.Count, Is.EqualTo(1),
                        "Unable to find a single element with the selector '{0}'", selector);
            return elementsFromPageByXPath[0];

        }

        #endregion



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

    }
}