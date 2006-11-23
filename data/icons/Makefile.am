noinst_DATA =					\
	face-sad.png				\
	face-smile.png				\
	favourites-image.png			\
	f-spot-favorite.png			\
	f-spot-other.png			\
	people-image.png			\
	person-image.png			\
	secure.png				\
	unknown-cover.png

install-data-local: install-icons
uninstall-local: uninstall-icons

gtk_update_icon_cache = gtk-update-icon-cache -f -t $(datadir)/icons/hicolor

icons = \
	last-exit-16.png \
	last-exit-22.png \
	last-exit-24.png \
	last-exit-scalable.svg

install-icons:
	@echo "Installing application icons..."
	for icon in $(icons); do \
		SIZE=`echo $$icon | cut -d- -f3 | cut -d. -f1`; \
		EXTENSION=`echo $$icon | cut -d. -f2`; \
		if test "$$SIZE" = "scalable"; then \
			SIZE="scalable"; \
		else \
			SIZE="$${SIZE}x$${SIZE}"; \
		fi; \
		mkdir -p $(DESTDIR)$(datadir)/icons/hicolor/$$SIZE/apps; \
		$(INSTALL_DATA) $(srcdir)/$$icon $(DESTDIR)$(datadir)/icons/hicolor/$$SIZE/apps/last-exit.$$EXTENSION; \
	done
	@-if test -z "$(DESTDIR)"; then \
		echo "Updating GTK icon cache."; \
		$(gtk_update_icon_cache); \
	else \
		echo "*** Icon cache not updated.  After install, run this:"; \
		echo "***   $(gtk_update_icon_cache)"; \
	fi

uninstall-icons:
	@echo "Uninstalling application icons..."
	for icon in $(icons); do \
		SIZE=`echo $$icon | cut -d- -f2 | cut -d. -f1`; \
		EXTENSION=`echo $$icon | cut -d. -f2`; \
		if test "$$SIZE" = "scalable"; then \
			SIZE="scalable"; \
		else \
			SIZE="$${SIZE}x$${SIZE}"; \
		fi; \
		rm -f $(DESTDIR)$(datadir)/icons/hicolor/$$SIZE/apps/last-exit.$$EXTENSION; \
	done

EXTRA_DIST = \
	$(noinst_DATA) \
	last-exit-16.png last-exit-22.png last-exit-24.png last-exit-scalable.svg

