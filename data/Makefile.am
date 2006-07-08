SUBDIRS=glade icons ui

schema_in_files = last-exit.schemas.in		\
		  lastfm.schemas.in

schemadir = $(GCONF_SCHEMA_FILE_DIR)
schema_DATA = $(schema_in_files:.schemas.in=.schemas)

@INTLTOOL_SCHEMAS_RULE@

desktopdir = $(datadir)/applications
desktop_in_files=last-exit.desktop.in
desktop_DATA=$(desktop_in_files:.desktop.in=.desktop)
@INTLTOOL_DESKTOP_RULE@

icondir = $(datadir)/icons/hicolor/24x24/apps
icon_DATA = icons/last-exit-24.png
svgicondir = $(datadir)/icons/hicolor/scalable/apps
svgicon_DATA = icons/last-exit.svg

gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

EXTRA_DIST =			\
	$(schema_in_files)	\
	$(desktop_in_files)	\
	$(icon_DATA)		\
	$(svgicon_DATA)

DISTCLEANFILES =		\
	$(schema_DATA)		\
	$(desktop_DATA)

if GCONF_SCHEMAS_INSTALL
install-data-local:
	if test -z "$(DESTDIR)" ; then \
		for p in $(schema_DATA) ; do \
			GCONF_CONFIG_SOURCE=$(GCONF_SCHEMA_CONFIG_SOURCE) $(GCONFTOOL) --makefile-install-rule $$p; \
		done \
	fi
else
install-data-local:
endif

install-data-hook:
	@-if test -z "$(DESTDIR)"; then \
		echo "Updating Gtk icon cache."; \
		$(gtk_update_icon_cache); \
	else \
		echo "*** Icon cache not updated.  After install, run this:"; \
		echo "***   $(gtk_update_icon_cache)"; \
	fi