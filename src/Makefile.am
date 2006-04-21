CSC = $(MCS) $(MCS_FLAGS) $(CSFLAGS)

TARGET = last-exit.exe
WRAPPER = last-exit

LASTEXIT_CSFILES =				\
	$(srcdir)/Actions.cs			\
	$(srcdir)/Artist.cs			\
	$(srcdir)/Config.cs			\
	$(srcdir)/Driver.cs			\
	$(srcdir)/Fan.cs			\
	$(srcdir)/FindStation.cs		\
	$(srcdir)/FirstRunDialog.cs		\
	$(srcdir)/FMConnection.cs		\
	$(srcdir)/FMRequest.cs			\
	$(srcdir)/IconEntry.cs			\
	$(srcdir)/IconEntryPosition.cs		\
	$(srcdir)/IconPressedHandler.cs		\
	$(srcdir)/IconReleasedHandler.cs	\
	$(srcdir)/ImageLoader.cs		\
	$(srcdir)/InfoWindow.cs			\
	$(srcdir)/MagicCoverImage.cs		\
	$(srcdir)/Player.cs			\
	$(srcdir)/PlayerWindow.cs		\
	$(srcdir)/RecentStations.cs		\
	$(srcdir)/SignalUtils.cs		\
	$(srcdir)/Song.cs			\
	$(srcdir)/StationStore.cs		\
	$(srcdir)/StockIcons.cs			\
	$(srcdir)/StringUtils.cs		\
	$(srcdir)/Tag.cs			\
	$(srcdir)/TagDialog.cs			\
	$(srcdir)/TagSelector.cs		\
	$(srcdir)/TagView.cs			\
	$(srcdir)/UrlActivatedHandler.cs	\
	$(srcdir)/UrlLabel.cs			\
	$(srcdir)/VolumeButton.cs

LASTEXIT_ASSEMBLIES =				\
	-r:Mono.Posix

LASTEXIT_RESOURCES =				\
	-resource:$(top_srcdir)/data/glade/FindStation.glade	\
	-resource:$(top_srcdir)/data/glade/FirstRunDialog.glade	\
	-resource:$(top_srcdir)/data/glade/InfoWindow.glade	\
	-resource:$(top_srcdir)/data/glade/PlayerWindow.glade	\
	-resource:$(top_srcdir)/data/glade/SimilarResults.glade	\
	-resource:$(top_srcdir)/data/glade/TagDialog.glade	\
	-resource:$(top_srcdir)/data/glade/TagResults.glade	\
	-resource:$(top_srcdir)/data/icons/face-sad.png		\
	-resource:$(top_srcdir)/data/icons/face-smile.png	\
	-resource:$(top_srcdir)/data/icons/favourites-image.png	\
	-resource:$(top_srcdir)/data/icons/people-image.png	\
	-resource:$(top_srcdir)/data/icons/person-image.png	\
	-resource:$(top_srcdir)/data/icons/secure.png		\
	-resource:$(top_srcdir)/data/icons/unknown-cover.png

$(WRAPPER): $(srcdir)/$(WRAPPER).in
	sed -e"s|\@prefix\@|$(prefix)|g"		\
	    -e"s|\@pkglibdir\@|$(pkglibdir)|g"		\
	    -e"s|\@target\@|$(TARGET)|g"		\
	    -e"s|\@mono\@|$(MONO)|g"			\
	    -e"s|\@mono_flags\@|$(MONO_FLAGS)|g"	\
	    < $^ > $@

$(TARGET).config: $(srcdir)/$(TARGET).config.in
	sed -e "s|\@pkglibdir\@|$(pkglibdir)|" 		\
	    < $^ > $@

$(TARGET): $(LASTEXIT_CSFILES)
	$(CSC) -out:$@ $(LASTEXIT_CSFILES) $(LASTEXIT_ASSEMBLIES) $(LASTEXIT_RESOURCES) $(GTKSHARP_LIBS)

all: $(TARGET)

lastexitlibdir = $(pkglibdir)
lastexitlib_DATA = $(TARGET) $(TARGET).config

bin_SCRIPTS = $(WRAPPER)

EXTRA_DIST =					\
	$(LASTEXIT_CSFILES)			\
	$(WRAPPER).in				\
	$(TARGET).config.in

CLEANFILES = 					\
	$(TARGET)				\
	$(TARGET).config			\
	$(WRAPPER)
