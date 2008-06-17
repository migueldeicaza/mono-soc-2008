reporterdir = $(pkglibdir)
reporter_SCRIPTS = reporter.exe 
reporter_DATA = 
EXTRA_DIST = 
CLEANFILES = reporter.exe 
DISTCLEANFILES = Makefile.in TestResult.xml

reporter_sources = \
	Driver.cs 

reporter_build_sources = $(addprefix $(srcdir)/, $(reporter_sources))

reporter.exe: $(reporter_build_sources) 
	$(GMCS) -target:exe -out:$@ $(reporter_build_sources)
