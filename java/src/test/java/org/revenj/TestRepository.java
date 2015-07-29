package org.revenj;

import gen.model.Boot;
import gen.model.Seq.Next;
import gen.model.test.*;
import gen.model.test.repositories.*;
import org.junit.Assert;
import org.junit.Test;
import org.revenj.patterns.*;

import java.io.IOException;
import java.math.BigDecimal;
import java.sql.*;
import java.time.LocalDate;
import java.util.*;

public class TestRepository {

	@Test
	public void repositoryTest() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		PersistableRepository<Composite> repository = locator.resolve(CompositeRepository.class);
		Composite co = new Composite();
		UUID id = UUID.randomUUID();
		co.setId(id);
		Simple so = new Simple();
		so.setNumber(5);
		so.setText("test me ' \\ \" now");
		co.setSimple(so);
		String uri = repository.insert(co);
		Assert.assertEquals(id.toString(), uri);
		Optional<Composite> found = repository.find(uri);
		Assert.assertTrue(found.isPresent());
		Composite co2 = found.get();
		Assert.assertEquals(co.getId(), co2.getId());
		Assert.assertEquals(co.getSimple().getNumber(), co2.getSimple().getNumber());
		Assert.assertEquals(co.getSimple().getText(), co2.getSimple().getText());
		co2.getSimple().setNumber(6);
		repository.update(co, co2);
		found = repository.find(uri);
		Assert.assertTrue(found.isPresent());
		Composite co3 = found.get();
		Assert.assertEquals(co2.getId(), co3.getId());
		Assert.assertEquals(co2.getSimple().getNumber(), co3.getSimple().getNumber());
		Assert.assertEquals(co2.getSimple().getText(), co3.getSimple().getText());
		repository.delete(co3);
		found = repository.find(uri);
		Assert.assertFalse(found.isPresent());
	}

	@Test
	public void eventTest() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		DomainEventStore<Clicked> store = locator.resolve(ClickedRepository.class);
		Clicked cl = new Clicked().setBigint(Long.MAX_VALUE).setDate(LocalDate.now()).setNumber(BigDecimal.valueOf(11.22));
		String uri = store.submit(cl);
		Optional<Clicked> cl2o = store.find(uri);
		Assert.assertTrue(cl2o.isPresent());
		Clicked cl2 = cl2o.get();
		Assert.assertEquals(uri, cl2.getURI());
		Assert.assertEquals(cl.getBigint(), cl2.getBigint());
		Assert.assertEquals(cl.getDate(), cl2.getDate());
		Assert.assertEquals(cl.getNumber(), cl2.getNumber());
	}

	@Test
	public void sequenceTest() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		PersistableRepository<Next> repository = new Generic<PersistableRepository<Next>>() {
		}.resolve(locator);
		Next next = new Next();
		int id = next.getID();
		String uri = repository.insert(next);
		Assert.assertNotEquals(id, next.getID());
		Optional<Next> found = repository.find(uri);
		Assert.assertTrue(found.isPresent());
		Assert.assertEquals(next.getID(), found.get().getID());
	}

	@Test
	public void simpleRootSearch() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		PersistableRepository<Next> repository = new Generic<PersistableRepository<Next>>() {
		}.resolve(locator);
		Next next = new Next();
		repository.insert(next);
		List<Next> found = repository.search(1);
		Assert.assertEquals(1, found.size());
	}

	@Test
	public void simpleEventSearch() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		DomainEventStore<Clicked> store = locator.resolve(ClickedRepository.class);
		Clicked cl1 = new Clicked().setBigint(Long.MIN_VALUE).setDate(LocalDate.now()).setNumber(BigDecimal.valueOf(11.22));
		Clicked cl2 = new Clicked().setBigint(Long.MAX_VALUE).setDate(LocalDate.now()).setNumber(BigDecimal.valueOf(11.22));
		String[] uris = store.submit(Arrays.asList(cl1, cl2));
		Assert.assertEquals(2, uris.length);
		List<Clicked> found = store.search(2);
		Assert.assertEquals(2, found.size());
	}

	@Test
	public void specificationRootSearch() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		PersistableRepository<Next> repository = new Generic<PersistableRepository<Next>>() {
		}.resolve(locator);
		String uri = repository.insert(new Next());
		int id = Integer.parseInt(uri);
		List<Next> found = repository.search(new Next.BetweenIds(id, id));
		Assert.assertEquals(1, found.size());
	}

	@Test
	public void specificationEventSearch() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		DomainEventStore<Clicked> store = locator.resolve(ClickedRepository.class);
		Random rnd = new Random();
		Long rndLong = rnd.nextLong();
		BigDecimal rndDecimal = BigDecimal.valueOf(rnd.nextDouble());
		Clicked cl = new Clicked().setBigint(rndLong).setDate(LocalDate.now()).setNumber(rndDecimal).setEn(En.B);
		String[] uris = store.submit(Collections.singletonList(cl));
		Assert.assertEquals(1, uris.length);
		List<Clicked> found = store.search(new Clicked.BetweenNumbers(rndDecimal, Collections.singleton(rndDecimal), En.B));
		Assert.assertEquals(1, found.size());
	}

	@Test
	public void specificationSnowflakeFind() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		CompositeRepository repository = locator.resolve(CompositeRepository.class);
		CompositeListRepository listRepository = locator.resolve(CompositeListRepository.class);
		String uri = repository.insert(new Composite().setSimple(new Simple().setNumber(1234)));
		CompositeList list = listRepository.find(uri).get();
		Assert.assertEquals(1234, list.getSimple().getNumber());
	}

	@Test
	public void lazyLoadTest() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		CompositeRepository compRepository = locator.resolve(CompositeRepository.class);
		String uri = compRepository.insert(new Composite().setSimple(new Simple().setNumber(5432)));
		Composite c = compRepository.find(uri).get();
		Assert.assertEquals(5432, c.getSimple().getNumber());
		LazyLoadRepository llRepository = locator.resolve(LazyLoadRepository.class);
		uri = llRepository.insert(new LazyLoad().setComp(c));
		LazyLoad ll = llRepository.find(uri).get();
		Assert.assertEquals(c.getURI(), ll.getCompURI());
		Assert.assertEquals(5432, ll.getComp().getSimple().getNumber());
	}

	@Test
	public void detailTest() throws IOException, SQLException {
		ServiceLocator locator = Boot.configure("jdbc:postgresql://localhost:5432/revenj");
		SingleDetailRepository sdRepository = locator.resolve(SingleDetailRepository.class);
		String uri = sdRepository.insert(new SingleDetail());
		SingleDetail sd = sdRepository.find(uri).get();
		Assert.assertEquals(0, sd.getDetails().length);
		Assert.assertEquals(0, sd.getDetailsURI().length);
		LazyLoadRepository llRepository = locator.resolve(LazyLoadRepository.class);
		uri = llRepository.insert(new LazyLoad().setSd(sd));
		SingleDetail sd2 = sdRepository.find(sd.getURI()).get();
		Assert.assertEquals(1, sd2.getDetails().length);
		Assert.assertEquals(1, sd2.getDetailsURI().length);
		Assert.assertEquals(uri, sd2.getDetailsURI()[0]);
		Assert.assertEquals(uri, sd2.getDetails()[0].getURI());
	}
}