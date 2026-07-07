using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfAppDigitalTwinsRepository
{
    class TextBoxLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = $"{DateTime.Now:HH:mm:ss} [{logLevel}] : {formatter(state, exception)}";
            dispatcher.Invoke(() =>
            {
                textBlock.Text +=message + Environment.NewLine;
                scrollViewer.ScrollToEnd();
            });
        }

        public TextBoxLogger(TextBlock textBlock, ScrollViewer scrollViewer, Dispatcher dispatcher, string category)
        {
            this.textBlock = textBlock;
            this.scrollViewer = scrollViewer;
            this.dispatcher = dispatcher;
            this.category = category;
        }

        protected readonly TextBlock textBlock;
        private ScrollViewer scrollViewer;
        protected readonly Dispatcher dispatcher;
        protected readonly string category;
    }

    public class TextBoxLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TextBoxLogger(textBlock, scrollViewer, dispatcher, categoryName);

        public void Dispose()
        {
            ;        }

        public TextBoxLoggerProvider(TextBlock textBlock, ScrollViewer scrollViewer, Dispatcher dispatcher)
        {
            this.textBlock = textBlock;
            this.scrollViewer = scrollViewer;
            this.dispatcher = dispatcher;
        }

        private readonly TextBlock textBlock;
        private readonly ScrollViewer scrollViewer;
        private readonly Dispatcher dispatcher;
    }
}

