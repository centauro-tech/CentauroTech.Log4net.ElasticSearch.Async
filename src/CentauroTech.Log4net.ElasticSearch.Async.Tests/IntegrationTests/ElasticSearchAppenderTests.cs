﻿namespace CentauroTech.Log4net.ElasticSearch.Async.Tests.IntegrationTests
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using CentauroTech.Log4net.ElasticSearch.Async.Models;
    using CentauroTech.Log4net.ElasticSearch.Async.Tests.Infrastructure;

    using Nest;

    using Xunit;
    using Xunit.Sdk;
    using log4net;

    [Collection("IndexCollection")]
    public class ElasticSearchAppenderTests
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ElasticSearchAppenderTests));

        private IntegrationTestFixture testFixture;
        private ElasticClient elasticClient;

        public ElasticSearchAppenderTests(IntegrationTestFixture testFixture)
        {
            this.testFixture = testFixture;
            this.elasticClient = testFixture.Client;
        }

        [Fact(Skip = "It was already wrong at master")]
        public void Can_create_an_event_from_log4net()
        {
            var message = Faker.Lorem.Words(1).Single();

            _log.Info(message, new ApplicationException(Faker.Lorem.Words(1).Single()));

            Retry.Ignoring<XunitException>(() =>
            {
                var logEntries =
                    this.elasticClient.Search<logEvent>(s => s.Query(qd => qd.Term(le => le.message, message)));

                logEntries.Total.Should().Be(1);
            });
        }

        [Fact(Skip = "It was already wrong at master")]
        public void Can_create_error_event_from_log4net()
        {
            var message = Faker.Lorem.Words(1).Single();
            try
            {
                this.ThrowException();
            }
            catch (Exception ex)
            {
                _log.Error(message, ex);
            }

            Retry.Ignoring<XunitException>(() =>
            {
                var logEntries =
                    this.elasticClient.Search<logEvent>(s => s.Query(qd => qd.Term(le => le.message, message)));

                logEntries.Total.Should().Be(1);
            });
        }

        [Fact(Skip = "It was already wrong at master")]
        public void Can_create_error_event_from_log4net_with_nested_exception()
        {
            var message = Faker.Lorem.Words(1).Single();
            try
            {
                this.ThrowNestedException();
            }
            catch (Exception ex)
            {
                _log.Error(message, ex);
            }

            Retry.Ignoring<XunitException>(() =>
            {
                var logEntries =
                    this.elasticClient.Search<logEvent>(s => s.Query(qd => qd.Term(le => le.message, message)));

                logEntries.Total.Should().Be(1);
            });
        }

        [Fact(Skip = "It was already wrong at master")]
        public void Global_context_properties_are_logged()
        {
            const string globalPropertyName = "globalProperty";

            var globalProperty = Faker.Lorem.Sentence(2);
            var message = Faker.Lorem.Words(1).Single();

            GlobalContext.Properties[globalPropertyName] = globalProperty;

            _log.Info(message);

            Retry.Ignoring<XunitException>(() =>
                {
                    var logEntries =
                        this.elasticClient.Search<logEvent>(sd => sd.Query(qd => qd.Term(le => le.message, message)));

                    logEntries.Total.Should().Be(1);

                    var actualLogEntry = logEntries.Documents.First();

                    actualLogEntry.properties[globalPropertyName].Should().Be(globalProperty);
                });
        }

        [Fact(Skip = "It was already wrong at master")]
        public void Thread_context_properties_are_logged()
        {
            const string threadPropertyName = "threadProperty";

            var threadProperty = Faker.Lorem.Sentence(2);
            var message = Faker.Lorem.Words(1).Single();

            ThreadContext.Properties[threadPropertyName] = threadProperty;

            _log.Info(message);

            Retry.Ignoring<XunitException>(() =>
                {
                    var logEntries =
                        this.elasticClient.Search<logEvent>(sd => sd.Query(qd => qd.Term(le => le.message, message)));

                    logEntries.Total.Should().Be(1);

                    var actualLogEntry = logEntries.Documents.First();

                    actualLogEntry.properties[threadPropertyName].Should().Be(threadProperty);
                }, 20, 1000);
        }

        [Fact(Skip = "LogicalThreadContext properties cause SerializationException")]
        public void Local_thread_context_properties_cause_error()
        {
            const string localThreadPropertyName = "logicalThreadProperty";

            var localTreadProperty = Faker.Lorem.Sentence(2);
            var message = Faker.Lorem.Words(1).Single();

            LogicalThreadContext.Properties[localThreadPropertyName] = localTreadProperty;

            _log.Info(message);

            Retry.Ignoring<XunitException>(() =>
                {
                    var logEntries =
                        this.elasticClient.Search<logEvent>(sd => sd.Query(qd => qd.Term(le => le.message, message)));

                logEntries.Total.Should().Be(1);

                var actualLogEntry = logEntries.Documents.First();

                actualLogEntry.properties[localThreadPropertyName].Should().Be(localTreadProperty);
            });
        }

        void ThrowException()
        {
            throw new InvalidOperationException("thrown from ThrowException.");
        }

        void ThrowNestedException()
        {
            try
            {
                this.ThrowException();
            }
            catch (Exception ex)
            {
                throw new Exception("thrown from ThrowNestedException.", ex);
            }
        }

    }
}
