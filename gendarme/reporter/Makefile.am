reporterdir = $(pkglibdir)
reporter_SCRIPTS = reporter.exe 
reporter_DATA = 
EXTRA_DIST = 
CLEANFILES = reporter.exe Test.Reporter.dll *.mdb TestResult.xml 
DISTCLEANFILES = Makefile.in TestResult.xml

reporter_sources = \
	Driver.cs \
	Pipeline.cs \
	IAction.cs \
	AddXSLProcessingInstructionAction.cs \
	GenerateMasterIndexAction.cs \
	ValidateInputXmlAction.cs \
	WriteToFileAction.cs \
	GenerateDefectsPerAssemblyAction.cs \
	FilterBySeverityAction.cs \
	IPipeline.cs \
	DefaultPipeline.cs

reporter_resources = \
	gendarme-output.xsd

reporter_test_sources = \
	PipelineTest.cs \
	AddXSLProcessingInstructionActionTest.cs\
	GenerateMasterIndexActionTest.cs \
	ValidateInputXmlActionTest.cs \
	WriteToFileActionTest.cs \
	GenerateDefectsPerAssemblyActionTest.cs \
	FilterBySeverityActionTest.cs

reporter_build_sources = $(addprefix $(srcdir)/, $(reporter_sources))
reporter_test_build_sources = $(addprefix $(srcdir)/Test/, $(reporter_test_sources))
reporter_build_resources = $(foreach res, $(reporter_resources), $(addprefix -resource:,$(res)),$(notdir $(res)))

reporter.exe: $(reporter_build_sources) $(reporter_resources) 
	$(GMCS) -target:exe -debug -r:System.Xml.Linq $(reporter_build_resources) -out:$@ $(reporter_build_sources)

Test.Reporter.dll: $(reporter_test_build_sources) reporter.exe
	$(GMCS) -target:library -debug -pkg:mono-nunit -r:reporter.exe -r:System.Xml.Linq -out:$@ $(reporter_test_build_sources)

test: Test.Reporter.dll

run-test: test
	nunit-console2 /xml=TestResult Test.Reporter.dll

run-gendarme: reporter.exe
	gendarme reporter.exe 
